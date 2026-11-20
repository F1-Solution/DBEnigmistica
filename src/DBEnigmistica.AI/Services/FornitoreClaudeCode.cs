using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.AI.Services;

/// <summary>Impostazioni dell'adattatore Claude Code.</summary>
public sealed record ImpostazioniClaudeCode(
    string Eseguibile,
    string Modello,
    TimeSpan Timeout,
    int MaxTentativi = 3,
    TimeSpan? AttesaIniziale = null,
    int? MassimoRigaComando = null);

/// <summary>Adattatore Claude Code non interattivo (ADR 004, 010): processo nativo, prompt su stdin, risposta JSON validata.</summary>
public sealed class FornitoreClaudeCode(
    ImpostazioniClaudeCode impostazioni,
    IValidatoreSchemaJson validatore,
    TimeProvider? tempo = null,
    Func<TimeSpan, CancellationToken, Task>? attesa = null) : IFornitoreAiEstrazione
{
    public const string Id = "claude-code";
    private const string NomeImmagine = "pagina.png";
    private const int RigaComandoMassimaWindows = 32_000;
    private const int RigaComandoMassimaAltri = 120_000;
    private const string RigaImmagine =
        "L'immagine della pagina è nel file pagina.png nella cartella corrente: leggila con lo strumento Read.";

    // Variabili che cambierebbero autenticazione o fornitore e quindi addebiterebbero a consumo (D5).
    private static readonly string[] VariabiliDaTogliere =
    [
        "ANTHROPIC_API_KEY",
        "ANTHROPIC_AUTH_TOKEN",
        "ANTHROPIC_BASE_URL",
        "ANTHROPIC_PROFILE",
        "ANTHROPIC_FEDERATION_RULE_ID",
        "ANTHROPIC_ORGANIZATION_ID",
        "CLAUDE_CODE_USE_BEDROCK",
        "CLAUDE_CODE_USE_VERTEX",
        "CLAUDE_CODE_USE_FOUNDRY",
    ];

    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;
    private readonly Func<TimeSpan, CancellationToken, Task> _attesa = attesa ?? ((durata, ct) => Task.Delay(durata, ct));

    public string IdFornitore => Id;

    public string Modello => impostazioni.Modello;

    public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        var sessione = new Sessione(richiesta, impostazioni.Modello);
        var eseguibile = impostazioni.Eseguibile.Trim();

        // Si toglie anche il punto o lo spazio finale: Windows li ignora e "claude.cmd." sarebbe comunque un .cmd.
        var nome = eseguibile.TrimEnd('.', ' ');
        if (nome.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) || nome.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
        {
            return new RispostaEstrazionePagina(
                EsitoEstrazione.ErroreDefinitivo, null, "Serve l'eseguibile nativo di Claude Code, non un file .cmd o .bat", null, []);
        }

        var argomenti = CostruisciArgomenti(richiesta.JsonSchema, out var erroreArgomenti);
        if (erroreArgomenti is not null)
            return new RispostaEstrazionePagina(EsitoEstrazione.ErroreDefinitivo, null, erroreArgomenti, null, []);

        sessione.Argomenti = argomenti;

        var cartella = Path.Combine(Path.GetTempPath(), "dbe-pagina-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(cartella);
        try
        {
            await File.WriteAllBytesAsync(Path.Combine(cartella, NomeImmagine), richiesta.ImmaginePng, cancellationToken);
            return await EstraiAsync(sessione, cartella, cancellationToken);
        }
        finally
        {
            Elimina(cartella);
        }
    }

    private async Task<RispostaEstrazionePagina> EstraiAsync(Sessione sessione, string cartella, CancellationToken ct)
    {
        // Il prompt del modello va inviato com'è, seguito solo dalla riga che indica l'immagine.
        var prompt = $"{sessione.Richiesta.Prompt}\n\n{RigaImmagine}";

        var tentativo = await ChiamaAsync(sessione, cartella, prompt, correzione: false, ct);
        if (tentativo.Uscita.Tipo != TipoUscitaClaudeCode.Successo)
            return DaUscita(sessione, tentativo, correzione: false);

        var errori = Errori(tentativo.Uscita, sessione.Richiesta);
        if (errori.Count == 0)
            return Fine(sessione, tentativo, EsitoEstrazione.Riuscita, false, tentativo.Uscita.Json, null);

        // Al massimo una richiesta di correzione se la risposta non passa lo schema.
        Registra(sessione, tentativo, EsitoEstrazione.RispostaNonValida, false, string.Join("; ", errori));
        var correzione = $"{prompt}\n\nLa risposta precedente non rispetta lo schema: {string.Join("; ", errori)}.\n"
                         + $"Risposta precedente: {tentativo.Uscita.Json}\nRispondi di nuovo con un JSON corretto.";
        tentativo = await ChiamaAsync(sessione, cartella, correzione, correzione: true, ct);
        if (tentativo.Uscita.Tipo != TipoUscitaClaudeCode.Successo)
            return DaUscita(sessione, tentativo, correzione: true);

        errori = Errori(tentativo.Uscita, sessione.Richiesta);
        return errori.Count == 0
            ? Fine(sessione, tentativo, EsitoEstrazione.Riuscita, true, tentativo.Uscita.Json, null)
            : Fine(sessione, tentativo, EsitoEstrazione.RispostaNonValida, true, null, string.Join("; ", errori));
    }

    private IReadOnlyList<string> Errori(UscitaClaudeCode uscita, RichiestaEstrazionePagina richiesta) =>
        uscita.Json is null
            ? [uscita.Messaggio ?? "Risposta vuota"]
            : validatore.Valida(uscita.Json, richiesta.JsonSchema);

    private static RispostaEstrazionePagina DaUscita(Sessione sessione, Tentativo tentativo, bool correzione)
    {
        var esito = tentativo.Uscita.Tipo switch
        {
            TipoUscitaClaudeCode.FinestraEsaurita => EsitoEstrazione.FinestraEsaurita,
            TipoUscitaClaudeCode.Transitorio => EsitoEstrazione.ErroreTransitorio,
            _ => EsitoEstrazione.ErroreDefinitivo,
        };
        return Fine(sessione, tentativo, esito, correzione, null, tentativo.Uscita.Messaggio);
    }

    private static RispostaEstrazionePagina Fine(
        Sessione sessione, Tentativo tentativo, EsitoEstrazione esito, bool correzione, string? json, string? messaggio)
    {
        Registra(sessione, tentativo, esito, correzione, messaggio);
        return new RispostaEstrazionePagina(
            esito, json, messaggio, tentativo.Uscita.Ripresa, [.. sessione.Chiamate], MetadatiChiamata.Somma(sessione.Chiamate));
    }

    // Aggiunge la riga del tentativo; il numero del tentativo è la sua posizione nell'esecuzione della pagina.
    private static void Registra(Sessione sessione, Tentativo tentativo, EsitoEstrazione esito, bool correzione, string? messaggio)
    {
        var uscita = tentativo.Uscita;
        var errore = esito == EsitoEstrazione.Riuscita ? null : messaggio;
        sessione.Chiamate.Add(new ChiamataAi(
            Guid.CreateVersion7(),
            tentativo.Avviata,
            tentativo.DurataMs,
            ChiamataAi.UsoEstrazione,
            Id,
            sessione.Modello,
            sessione.Richiesta.VersionePrompt,
            sessione.ImprontaPagina,
            sessione.Chiamate.Count + 1,
            correzione,
            esito,
            esito == EsitoEstrazione.FinestraEsaurita ? uscita.Ripresa : null,
            uscita.CodiceUscita,
            errore is { Length: > ChiamataAi.MassimoMessaggioErrore } ? errore[..ChiamataAi.MassimoMessaggioErrore] : errore,
            uscita.TokenIngresso,
            uscita.TokenUscita,
            uscita.TokenCacheCreazione,
            uscita.TokenCacheLettura,
            uscita.CostoUsd,
            uscita.CostoUsd is null ? null : "USD",
            uscita.CostoUsd is null ? null : ChiamataAi.CostoIndicativo,
            sessione.TestoNativoTroncato,
            tentativo.Grezza,
            tentativo.Grezza is null ? null : SHA256.HashData(Encoding.UTF8.GetBytes(tentativo.Grezza))));
    }

    // Retry con attesa crescente solo sugli errori transitori; l'ultimo tentativo lo registra il chiamante.
    private async Task<Tentativo> ChiamaAsync(Sessione sessione, string cartella, string prompt, bool correzione, CancellationToken ct)
    {
        var attesaIniziale = impostazioni.AttesaIniziale ?? TimeSpan.FromSeconds(2);
        var massimo = Math.Max(1, impostazioni.MaxTentativi);
        for (var numero = 1; ; numero++)
        {
            var avviata = _tempo.GetUtcNow();
            var cronometro = Stopwatch.StartNew();
            var (uscita, grezza) = await EseguiAsync(sessione, cartella, prompt, ct);
            var tentativo = new Tentativo(uscita, grezza, avviata, (int)cronometro.ElapsedMilliseconds);
            if (uscita.Tipo != TipoUscitaClaudeCode.Transitorio || numero >= massimo)
                return tentativo;

            Registra(sessione, tentativo, EsitoEstrazione.ErroreTransitorio, correzione, uscita.Messaggio);
            await _attesa(attesaIniziale * Math.Pow(2, numero - 1), ct);
        }
    }

    private sealed record Tentativo(UscitaClaudeCode Uscita, string? Grezza, DateTimeOffset Avviata, int DurataMs);

    private sealed class Sessione(RichiestaEstrazionePagina richiesta, string modello)
    {
        public RichiestaEstrazionePagina Richiesta { get; } = richiesta;

        public string Modello { get; } = modello;

        public byte[] ImprontaPagina { get; } = ChiamataAi.ImprontaDaEsadecimale(richiesta.ImprontaPagina);

        public bool TestoNativoTroncato { get; } = richiesta.TestoNativoTroncato;

        public IReadOnlyList<string> Argomenti { get; set; } = [];

        public List<ChiamataAi> Chiamate { get; } = [];
    }

    private async Task<(UscitaClaudeCode Uscita, string? Grezza)> EseguiAsync(
        Sessione sessione, string cartella, string prompt, CancellationToken ct)
    {
        var avvio = new ProcessStartInfo(impostazioni.Eseguibile.Trim())
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = cartella,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        foreach (var argomento in sessione.Argomenti)
            avvio.ArgumentList.Add(argomento);

        foreach (var variabile in VariabiliDaTogliere)
            avvio.Environment.Remove(variabile);

        Process processo;
        try
        {
            processo = Process.Start(avvio) ?? throw new InvalidOperationException("Avvio di Claude Code non riuscito");
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or FileNotFoundException)
        {
            return (new UscitaClaudeCode(TipoUscitaClaudeCode.Definitivo, null, $"Claude Code non avviabile: {ex.Message}", null, null, null, null), null);
        }

        using (processo)
        {
            var uscitaStandard = processo.StandardOutput.ReadToEndAsync();
            var erroriStandard = processo.StandardError.ReadToEndAsync();
            Osserva(uscitaStandard);
            Osserva(erroriStandard);
            using var scadenza = CancellationTokenSource.CreateLinkedTokenSource(ct);
            scadenza.CancelAfter(impostazioni.Timeout);

            try
            {
                await ScriviStdinAsync(processo, prompt, scadenza.Token);
                await processo.WaitForExitAsync(scadenza.Token);

                // Un nipote che tiene aperta la pipe non deve bloccare oltre il timeout.
                var testo = await uscitaStandard.WaitAsync(scadenza.Token);
                var errori = await erroriStandard.WaitAsync(scadenza.Token);
                return (UscitaClaudeCode.Analizza(testo, errori, processo.ExitCode, _tempo), testo);
            }
            catch (OperationCanceledException)
            {
                Chiudi(processo);
                if (ct.IsCancellationRequested)
                    throw;

                return (new UscitaClaudeCode(TipoUscitaClaudeCode.Transitorio, null, "Timeout di Claude Code", null, null, null, null), null);
            }
        }
    }

    private static async Task ScriviStdinAsync(Process processo, string prompt, CancellationToken ct)
    {
        try
        {
            await processo.StandardInput.WriteAsync(prompt.AsMemory(), ct);
            processo.StandardInput.Close();
        }
        catch (IOException)
        {
            // Il processo può uscire senza leggere tutto lo stdin: l'esito si legge dall'uscita.
        }
    }

    private static void Chiudi(Process processo)
    {
        try
        {
            processo.Kill(entireProcessTree: true);
            processo.WaitForExit(TimeSpan.FromSeconds(5));
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception)
        {
            // Il processo era già terminato.
        }
    }

    // Una lettura abbandonata dopo un timeout non deve lasciare eccezioni non osservate.
    private static void Osserva(Task<string> lettura) =>
        lettura.ContinueWith(t => _ = t.Exception, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);

    // Opzioni dalla documentazione ufficiale, da confermare con la prova reale (prova-claude-code-1.1.md).
    // Read è limitato al solo file dell'immagine; --setting-sources project evita le impostazioni utente (hook, apiKeyHelper).
    private IReadOnlyList<string> CostruisciArgomenti(string jsonSchema, out string? errore)
    {
        errore = null;
        string schema;
        try
        {
            schema = Minimizza(jsonSchema);
        }
        catch (JsonException)
        {
            errore = "Lo schema JSON della richiesta non è un JSON valido";
            return [];
        }

        string[] argomenti =
        [
            "-p", "--output-format", "json", "--json-schema", schema, "--model", impostazioni.Modello,
            "--tools", "Read", "--allowedTools", $"Read(./{NomeImmagine})", "--permission-mode", "dontAsk",
            "--setting-sources", "project",
            "--no-session-persistence", "--strict-mcp-config", "--disable-slash-commands",
        ];

        var massimo = impostazioni.MassimoRigaComando
                      ?? (OperatingSystem.IsWindows() ? RigaComandoMassimaWindows : RigaComandoMassimaAltri);
        var lunghezza = LunghezzaRigaComando(impostazioni.Eseguibile.Trim(), argomenti);
        if (lunghezza > massimo)
        {
            errore = $"Lo schema JSON è troppo lungo per la riga di comando di Claude Code: la riga sarebbe di {lunghezza} caratteri, "
                     + $"il massimo è {massimo} (schema di {schema.Length} caratteri dopo la minimizzazione).";
            return [];
        }

        return argomenti;
    }

    private static string Minimizza(string json)
    {
        using var documento = JsonDocument.Parse(json);
        using var memoria = new MemoryStream();
        using (var scrittore = new Utf8JsonWriter(memoria, new JsonWriterOptions { Indented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
            documento.RootElement.WriteTo(scrittore);

        return Encoding.UTF8.GetString(memoria.ToArray());
    }

    // Stima per eccesso della riga di comando: virgolette e barre rovesciate contano doppio, ogni argomento ha le virgolette.
    private static int LunghezzaRigaComando(string eseguibile, IEnumerable<string> argomenti) =>
        Lunghezza(eseguibile) + argomenti.Sum(a => 1 + Lunghezza(a));

    private static int Lunghezza(string valore) =>
        valore.Length + 2 + valore.Count(c => c is '"' or '\\');

    private static void Elimina(string cartella)
    {
        try
        {
            Directory.Delete(cartella, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Un file ancora trattenuto dal sistema non deve far fallire l'estrazione né nascondere il suo esito.
        }
    }
}

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
    private const string RigaImmagine =
        "L'immagine della pagina è nel file pagina.png nella cartella corrente: leggila con lo strumento Read.";

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

    private Task<(UscitaClaudeCode Uscita, string? Grezza)> EseguiAsync(
        Sessione sessione, string cartella, string prompt, CancellationToken ct) =>
        ProcessoClaudeCode.EseguiAsync(impostazioni.Eseguibile, sessione.Argomenti, cartella, prompt, impostazioni.Timeout, _tempo, ct);

    // Opzioni dalla documentazione ufficiale, da confermare con la prova reale (prova-claude-code-1.1.md).
    // Read è limitato al solo file dell'immagine; --setting-sources project evita le impostazioni utente (hook, apiKeyHelper).
    private IReadOnlyList<string> CostruisciArgomenti(string jsonSchema, out string? errore)
    {
        errore = null;
        string schema;
        try
        {
            schema = ProcessoClaudeCode.Minimizza(jsonSchema);
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
                      ?? (OperatingSystem.IsWindows() ? ProcessoClaudeCode.RigaComandoMassimaWindows : ProcessoClaudeCode.RigaComandoMassimaAltri);
        var lunghezza = ProcessoClaudeCode.LunghezzaRigaComando(impostazioni.Eseguibile.Trim(), argomenti);
        if (lunghezza > massimo)
        {
            errore = $"Lo schema JSON è troppo lungo per la riga di comando di Claude Code: la riga sarebbe di {lunghezza} caratteri, "
                     + $"il massimo è {massimo} (schema di {schema.Length} caratteri dopo la minimizzazione).";
            return [];
        }

        return argomenti;
    }

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

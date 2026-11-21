using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.AI.Services;

/// <summary>Adattatore Claude Code per la creazione: nessuna immagine, nessuno strumento, un candidato per chiamata.</summary>
public sealed class FornitoreClaudeCodeCreazione(
    ImpostazioniClaudeCode impostazioni,
    IValidatoreSchemaJson validatore,
    TimeProvider? tempo = null,
    Func<TimeSpan, CancellationToken, Task>? attesa = null) : IFornitoreAiCreazione
{
    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;
    private readonly Func<TimeSpan, CancellationToken, Task> _attesa = attesa ?? ((durata, ct) => Task.Delay(durata, ct));

    public string IdFornitore => FornitoreClaudeCode.Id;

    public string Modello => impostazioni.Modello;

    public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(richiesta);
        var eseguibile = impostazioni.Eseguibile.Trim();
        var nome = eseguibile.TrimEnd('.', ' ');
        if (nome.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) || nome.EndsWith(".bat", StringComparison.OrdinalIgnoreCase))
            return new RispostaCreazione(EsitoEstrazione.ErroreDefinitivo, null, "Serve l'eseguibile nativo di Claude Code, non un file .cmd o .bat", null, []);

        var argomenti = CostruisciArgomenti(richiesta.JsonSchema, out var errore);
        if (errore is not null)
            return new RispostaCreazione(EsitoEstrazione.ErroreDefinitivo, null, errore, null, []);

        var sessione = new Sessione(richiesta, impostazioni.Modello, argomenti);

        // Cartella di lavoro vuota: Claude Code non deve trovare né creare file.
        var cartella = Path.Combine(Path.GetTempPath(), "dbe-creazione-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(cartella);
        try
        {
            return await CreaInCartellaAsync(sessione, cartella, cancellationToken);
        }
        finally
        {
            Elimina(cartella);
        }
    }

    private async Task<RispostaCreazione> CreaInCartellaAsync(Sessione sessione, string cartella, CancellationToken ct)
    {
        var prompt = sessione.Richiesta.Prompt;
        var tentativo = await ChiamaAsync(sessione, cartella, prompt, false, ct);
        if (tentativo.Uscita.Tipo != TipoUscitaClaudeCode.Successo)
            return DaUscita(sessione, tentativo, false);

        var errori = Errori(tentativo.Uscita, sessione.Richiesta);
        if (errori.Count == 0)
            return Fine(sessione, tentativo, EsitoEstrazione.Riuscita, false, tentativo.Uscita.Json, null);

        Registra(sessione, tentativo, EsitoEstrazione.RispostaNonValida, false, string.Join("; ", errori));
        var correzione = $"{prompt}\n\nLa risposta precedente non rispetta lo schema: {string.Join("; ", errori)}.\n"
                         + $"Risposta precedente: {tentativo.Uscita.Json}\nRispondi di nuovo con un JSON corretto.";
        tentativo = await ChiamaAsync(sessione, cartella, correzione, true, ct);
        if (tentativo.Uscita.Tipo != TipoUscitaClaudeCode.Successo)
            return DaUscita(sessione, tentativo, true);

        errori = Errori(tentativo.Uscita, sessione.Richiesta);
        return errori.Count == 0
            ? Fine(sessione, tentativo, EsitoEstrazione.Riuscita, true, tentativo.Uscita.Json, null)
            : Fine(sessione, tentativo, EsitoEstrazione.RispostaNonValida, true, null, string.Join("; ", errori));
    }

    private IReadOnlyList<string> Errori(UscitaClaudeCode uscita, RichiestaCreazione richiesta) =>
        uscita.Json is null
            ? [uscita.Messaggio ?? "Risposta vuota"]
            : validatore.Valida(uscita.Json, richiesta.JsonSchema);

    private static RispostaCreazione DaUscita(Sessione sessione, Tentativo tentativo, bool correzione)
    {
        var esito = tentativo.Uscita.Tipo switch
        {
            TipoUscitaClaudeCode.FinestraEsaurita => EsitoEstrazione.FinestraEsaurita,
            TipoUscitaClaudeCode.Transitorio => EsitoEstrazione.ErroreTransitorio,
            _ => EsitoEstrazione.ErroreDefinitivo,
        };
        return Fine(sessione, tentativo, esito, correzione, null, tentativo.Uscita.Messaggio);
    }

    private static RispostaCreazione Fine(
        Sessione sessione, Tentativo tentativo, EsitoEstrazione esito, bool correzione, string? json, string? messaggio)
    {
        Registra(sessione, tentativo, esito, correzione, messaggio);
        return new RispostaCreazione(
            esito, json, messaggio, tentativo.Uscita.Ripresa, [.. sessione.Chiamate], MetadatiChiamata.Somma(sessione.Chiamate));
    }

    private static void Registra(Sessione sessione, Tentativo tentativo, EsitoEstrazione esito, bool correzione, string? messaggio)
    {
        var uscita = tentativo.Uscita;
        var errore = esito == EsitoEstrazione.Riuscita ? null : messaggio;
        sessione.Chiamate.Add(new ChiamataAi(
            Guid.CreateVersion7(),
            tentativo.Avviata,
            tentativo.DurataMs,
            ChiamataAi.UsoCreazione,
            FornitoreClaudeCode.Id,
            sessione.Modello,
            sessione.Richiesta.VersionePrompt,
            sessione.Richiesta.ImprontaByte(),
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
            false,
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
            var (uscita, grezza) = await ProcessoClaudeCode.EseguiAsync(
                impostazioni.Eseguibile, sessione.Argomenti, cartella, prompt, impostazioni.Timeout, _tempo, ct);
            var tentativo = new Tentativo(uscita, grezza, avviata, (int)cronometro.ElapsedMilliseconds);
            if (uscita.Tipo != TipoUscitaClaudeCode.Transitorio || numero >= massimo)
                return tentativo;

            Registra(sessione, tentativo, EsitoEstrazione.ErroreTransitorio, correzione, uscita.Messaggio);
            await _attesa(attesaIniziale * Math.Pow(2, numero - 1), ct);
        }
    }

    // Nessuno strumento (--tools vuoto), nessuna impostazione utente, nessuna sessione salvata.
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
            "--tools", string.Empty, "--permission-mode", "dontAsk",
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
            // Un file ancora trattenuto dal sistema non deve far fallire la creazione né nascondere il suo esito.
        }
    }

    private sealed record Tentativo(UscitaClaudeCode Uscita, string? Grezza, DateTimeOffset Avviata, int DurataMs);

    private sealed class Sessione(RichiestaCreazione richiesta, string modello, IReadOnlyList<string> argomenti)
    {
        public RichiestaCreazione Richiesta { get; } = richiesta;

        public string Modello { get; } = modello;

        public IReadOnlyList<string> Argomenti { get; } = argomenti;

        public List<ChiamataAi> Chiamate { get; } = [];
    }
}

using System.Security.Cryptography;
using System.Text;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Contatore delle chiamate reali di un'esecuzione.</summary>
public sealed class ContatoreChiamateAi
{
    private int _conteggio;

    public int Conteggio => Volatile.Read(ref _conteggio);

    public int Aggiungi(int numero) => Interlocked.Add(ref _conteggio, numero);
}

/// <summary>
/// Conta le chiamate reali (ogni tentativo, retry e correzione compresi) e rifiuta una pagina nuova a limite raggiunto;
/// una pagina già avviata può superare il limite con i suoi retry.
/// </summary>
public sealed class FornitoreAiConLimite(IFornitoreAiEstrazione interno, int? limite, ContatoreChiamateAi contatore) : IFornitoreAiEstrazione
{
    private readonly object _blocco = new();

    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        // Prenota la prima chiamata della pagina, così due pagine in parallelo non superano il limite.
        lock (_blocco)
        {
            if (limite is { } massimo && contatore.Conteggio >= massimo)
                throw new LimiteEsecuzioneRaggiuntoException();

            contatore.Aggiungi(1);
        }

        var risposta = await interno.EstraiPaginaAsync(richiesta, cancellationToken);
        contatore.Aggiungi(risposta.Chiamate.Count - 1);
        return risposta;
    }
}

/// <summary>Salva come Registrazione la risposta di ogni pagina riuscita o non valida.</summary>
public sealed class FornitoreAiRegistra(
    IFornitoreAiEstrazione interno,
    IArchivioRegistrazioni archivio,
    TimeProvider? tempo = null) : IFornitoreAiEstrazione
{
    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;

    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        var risposta = await interno.EstraiPaginaAsync(richiesta, cancellationToken);
        var grezza = risposta.Chiamate.LastOrDefault()?.RispostaGrezza;
        if (risposta.Esito is EsitoEstrazione.Riuscita or EsitoEstrazione.RispostaNonValida && grezza is not null)
        {
            var chiave = new ChiaveRegistrazione(richiesta.ImprontaPagina, richiesta.VersionePrompt, IdFornitore, Modello);
            await archivio.SalvaAsync(
                new Registrazione(
                    chiave, _tempo.GetUtcNow(), richiesta.Dpi, risposta.Esito, grezza,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(grezza))),
                    MetadatiChiamata.Somma(risposta.Chiamate)),
                cancellationToken);
        }

        return risposta;
    }
}

/// <summary>Restituisce la Registrazione della pagina senza alcuna chiamata, rifacendo lettura e validazione come dal vivo.</summary>
public sealed class FornitoreAiRiproduci(
    string idFornitore,
    string modello,
    IArchivioRegistrazioni archivio,
    IInterpreteRispostaGrezza interprete,
    IValidatoreSchemaJson validatore) : IFornitoreAiEstrazione
{
    public string IdFornitore => idFornitore;

    public string Modello => modello;

    public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        var chiave = new ChiaveRegistrazione(richiesta.ImprontaPagina, richiesta.VersionePrompt, idFornitore, modello);
        var registrata = await archivio.CaricaAsync(chiave, cancellationToken) ?? throw new RegistrazioneMancanteException(chiave);

        var sha = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(registrata.RispostaGrezza)));
        if (!string.Equals(sha, registrata.RispostaSha256, StringComparison.OrdinalIgnoreCase))
            throw new RegistrazioneNonValidaException($"{chiave}: la risposta non corrisponde al suo SHA-256, il file è stato modificato.");

        if (registrata.Dpi != richiesta.Dpi)
        {
            throw new RegistrazioneNonValidaException(
                $"{chiave}: registrata a {registrata.Dpi} DPI, ma la richiesta è a {richiesta.Dpi} DPI.");
        }

        // La riproduzione non fa chiamate e non scrive righe nel registro delle chiamate.
        var interpretata = interprete.Interpreta(registrata.RispostaGrezza);
        var errori = interpretata.Json is null
            ? [interpretata.Messaggio ?? "Risposta vuota"]
            : validatore.Valida(interpretata.Json, richiesta.JsonSchema);

        return errori.Count == 0
            ? new RispostaEstrazionePagina(EsitoEstrazione.Riuscita, interpretata.Json, null, null, [], registrata.Metadati)
            : new RispostaEstrazionePagina(EsitoEstrazione.RispostaNonValida, null, string.Join("; ", errori), null, [], registrata.Metadati);
    }
}

/// <summary>Scrive ogni chiamata nel registro delle chiamate AI.</summary>
public sealed class FornitoreAiConRegistroChiamate(IFornitoreAiEstrazione interno, IRegistroChiamateAi registro) : IFornitoreAiEstrazione
{
    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaEstrazionePagina> EstraiPaginaAsync(RichiestaEstrazionePagina richiesta, CancellationToken cancellationToken = default)
    {
        var risposta = await interno.EstraiPaginaAsync(richiesta, cancellationToken);
        foreach (var chiamata in risposta.Chiamate)
            await registro.RegistraAsync(chiamata, cancellationToken);

        return risposta;
    }
}

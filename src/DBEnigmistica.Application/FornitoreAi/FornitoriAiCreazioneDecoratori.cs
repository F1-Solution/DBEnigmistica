using System.Security.Cryptography;
using System.Text;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Permette di sapere in anticipo che il limite delle chiamate è già raggiunto, prima di creare un candidato.</summary>
public interface ILimiteChiamateAi
{
    bool LimiteRaggiunto { get; }
}

/// <summary>Conta le chiamate reali (retry e correzione compresi) e rifiuta una richiesta nuova a limite raggiunto.</summary>
public sealed class FornitoreAiCreazioneConLimite(IFornitoreAiCreazione interno, int? limite, ContatoreChiamateAi contatore)
    : IFornitoreAiCreazione, ILimiteChiamateAi
{
    private readonly object _blocco = new();

    public bool LimiteRaggiunto => limite is { } massimo && contatore.Conteggio >= massimo;

    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        lock (_blocco)
        {
            if (limite is { } massimo && contatore.Conteggio >= massimo)
                throw new LimiteEsecuzioneRaggiuntoException();

            contatore.Aggiungi(1);
        }

        var risposta = await interno.CreaAsync(richiesta, cancellationToken);
        contatore.Aggiungi(risposta.Chiamate.Count - 1);
        return risposta;
    }
}

/// <summary>Salva come Registrazione la risposta di ogni candidato riuscito o non valido.</summary>
public sealed class FornitoreAiCreazioneRegistra(
    IFornitoreAiCreazione interno,
    IArchivioRegistrazioniCreazione archivio,
    TimeProvider? tempo = null) : IFornitoreAiCreazione
{
    private readonly TimeProvider _tempo = tempo ?? TimeProvider.System;

    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        var risposta = await interno.CreaAsync(richiesta, cancellationToken);
        var grezza = risposta.Chiamate.LastOrDefault()?.RispostaGrezza;
        if (risposta.Esito is EsitoEstrazione.Riuscita or EsitoEstrazione.RispostaNonValida && grezza is not null)
        {
            var chiave = new ChiaveRegistrazione(richiesta.ImprontaRichiesta, richiesta.VersionePrompt, IdFornitore, Modello);
            await archivio.SalvaAsync(
                new RegistrazioneCreazione(
                    chiave, _tempo.GetUtcNow(), richiesta.Tipologia, richiesta.VersioneTipologia, risposta.Esito, grezza,
                    Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(grezza))),
                    MetadatiChiamata.Somma(risposta.Chiamate)),
                cancellationToken);
        }

        return risposta;
    }
}

/// <summary>Restituisce la Registrazione senza alcuna chiamata, rifacendo interpretazione e validazione come dal vivo.</summary>
public sealed class FornitoreAiCreazioneRiproduci(
    string idFornitore,
    string modello,
    IArchivioRegistrazioniCreazione archivio,
    IInterpreteRispostaGrezza interprete,
    IValidatoreSchemaJson validatore) : IFornitoreAiCreazione
{
    public string IdFornitore => idFornitore;

    public string Modello => modello;

    public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        var chiave = new ChiaveRegistrazione(richiesta.ImprontaRichiesta, richiesta.VersionePrompt, idFornitore, modello);
        var registrata = await archivio.CaricaAsync(chiave, cancellationToken) ?? throw new RegistrazioneMancanteException(chiave);

        var sha = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(registrata.RispostaGrezza)));
        if (!string.Equals(sha, registrata.RispostaSha256, StringComparison.OrdinalIgnoreCase))
            throw new RegistrazioneNonValidaException($"{chiave}: la risposta non corrisponde al suo SHA-256, il file è stato modificato.");

        if (registrata.Tipologia != richiesta.Tipologia || registrata.VersioneTipologia != richiesta.VersioneTipologia)
        {
            throw new RegistrazioneNonValidaException(
                $"{chiave}: registrata per {registrata.Tipologia} v{registrata.VersioneTipologia}, ma la richiesta è per {richiesta.Tipologia} v{richiesta.VersioneTipologia}.");
        }

        var interpretata = interprete.Interpreta(registrata.RispostaGrezza);
        var errori = interpretata.Json is null
            ? [interpretata.Messaggio ?? "Risposta vuota"]
            : validatore.Valida(interpretata.Json, richiesta.JsonSchema);

        return errori.Count == 0
            ? new RispostaCreazione(EsitoEstrazione.Riuscita, interpretata.Json, null, null, [], registrata.Metadati)
            : new RispostaCreazione(EsitoEstrazione.RispostaNonValida, null, string.Join("; ", errori), null, [], registrata.Metadati);
    }
}

/// <summary>Scrive ogni chiamata nel registro delle chiamate AI.</summary>
public sealed class FornitoreAiCreazioneConRegistroChiamate(IFornitoreAiCreazione interno, IRegistroChiamateAi registro) : IFornitoreAiCreazione
{
    public string IdFornitore => interno.IdFornitore;

    public string Modello => interno.Modello;

    public async Task<RispostaCreazione> CreaAsync(RichiestaCreazione richiesta, CancellationToken cancellationToken = default)
    {
        var risposta = await interno.CreaAsync(richiesta, cancellationToken);
        foreach (var chiamata in risposta.Chiamate)
            await registro.RegistraAsync(chiamata, cancellationToken);

        return risposta;
    }
}

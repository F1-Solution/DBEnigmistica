namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Registrazione di un candidato di creazione (progetto 1b.1, sezione 1.6); la chiave usa l'impronta della richiesta.</summary>
public sealed record RegistrazioneCreazione(
    ChiaveRegistrazione Chiave,
    DateTimeOffset RegistrataIl,
    string Tipologia,
    int VersioneTipologia,
    EsitoEstrazione Esito,
    string RispostaGrezza,
    string RispostaSha256,
    MetadatiChiamata Metadati);

/// <summary>Porta delle Registrazioni di creazione su disco.</summary>
public interface IArchivioRegistrazioniCreazione
{
    Task SalvaAsync(RegistrazioneCreazione registrazione, CancellationToken cancellationToken = default);

    /// <summary>Restituisce la Registrazione, o <c>null</c> se non esiste; un file illeggibile lancia <see cref="RegistrazioneNonValidaException"/>.</summary>
    Task<RegistrazioneCreazione?> CaricaAsync(ChiaveRegistrazione chiave, CancellationToken cancellationToken = default);
}

/// <summary>Apre l'archivio delle Registrazioni di creazione in una cartella.</summary>
public interface IFabbricaArchivioRegistrazioniCreazione
{
    IArchivioRegistrazioniCreazione Apri(string cartella);
}

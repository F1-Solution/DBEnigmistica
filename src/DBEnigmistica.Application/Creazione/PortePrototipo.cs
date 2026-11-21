using DBEnigmistica.Application.FornitoreAi;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Catalogo dei descrittori letti dal disco; il proprietario ne approva le regole.</summary>
public interface ICatalogoTipologie
{
    /// <summary>Il descrittore della versione indicata, o <c>null</c> se non esiste.</summary>
    Task<DescrittoreTipologia?> TrovaAsync(string id, int versione, CancellationToken cancellationToken = default);
}

/// <summary>Risultato di un generatore algoritmico: contenuto valido per lo schema, seme e versione del generatore.</summary>
public sealed record RisultatoAlgoritmico(string ContenutoJson, long Seme, string VersioneGeneratore);

/// <summary>Generatori algoritmici dei giochi logici e dello schema del cruciverba (agente C), dietro una porta.</summary>
public interface IGeneratoriAlgoritmici
{
    bool Supporta(string tipologia);

    /// <summary>Versione del generatore della Tipologia, per esempio <c>sudoku-1</c>.</summary>
    string VersioneGeneratore(string tipologia);

    RisultatoAlgoritmico Genera(string tipologia, BriefCreazione brief, long seme);

    SchemaCaselle GeneraSchemaCaselle(int dimensione, long seme);
}

/// <summary>Seme dei generatori: casuale in produzione, fisso nei test.</summary>
public interface ISorgenteSemi
{
    long Prossimo();
}

/// <summary>Esegue le operazioni di un caso d'uso in una sola transazione, così righe ed eventi riescono o falliscono insieme.</summary>
public interface ITransazionePrototipo
{
    Task<T> EseguiAsync<T>(Func<Task<T>> operazione, CancellationToken cancellationToken = default);
}

/// <summary>Persistenza dei candidati e delle loro chiamate (schema <c>prototipo</c>, ruolo <c>dbe_creazione</c>).</summary>
public interface ICandidatiRepository
{
    /// <summary>Aggiunge il candidato assegnando l'ordine di generazione progressivo della Tipologia.</summary>
    Task<Candidato> AggiungiAsync(Candidato candidato, CancellationToken cancellationToken = default);

    Task AggiornaAsync(Candidato candidato, CancellationToken cancellationToken = default);

    /// <summary>Prende il lucchetto della generazione per la Tipologia fino alla fine della transazione: serializza varianti e ordini.</summary>
    Task BloccaGenerazioneAsync(CancellationToken cancellationToken = default);

    /// <summary>Blocca la riga del candidato fino alla fine della transazione: due esiti in parallelo non si sovrappongono.</summary>
    Task BloccaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Dimentica le modifiche non salvate: dopo una transazione fallita nessuna riga sbagliata va riprovata.</summary>
    void ScartaModifiche();

    Task<Candidato?> TrovaAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Il primo candidato in <c>in_revisione</c> o <c>da_rivedere</c> per ordine di generazione.</summary>
    Task<Candidato?> ProssimoDaRivedereAsync(string tipologia, CancellationToken cancellationToken = default);

    /// <summary>I brief (JSON) di tutti i candidati della Tipologia, qualunque sia lo stato.</summary>
    Task<IReadOnlyList<string>> BriefDellaTipologiaAsync(string tipologia, CancellationToken cancellationToken = default);

    Task AggiungiChiamateAsync(Guid candidatoId, IReadOnlyList<ChiamataAi> chiamate, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChiamataAi>> ChiamateAsync(Guid candidatoId, CancellationToken cancellationToken = default);

    /// <summary>Azzera il testo delle risposte grezze (restano SHA-256 e metadati); restituisce le righe toccate.</summary>
    Task<int> CancellaRisposteGrezzeAsync(Guid? candidatoId, CancellationToken cancellationToken = default);

    /// <summary>Porta a <c>errore_generazione</c> i candidati rimasti <c>in_generazione</c> da prima di <paramref name="prima"/>.</summary>
    Task<int> RiparaInGenerazioneAsync(DateTime prima, CancellationToken cancellationToken = default);
}

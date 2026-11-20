using System.Text.RegularExpressions;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>Chiave di una Registrazione (progetto 1.1, sezione 6.2); ogni componente è validato perché diventa un percorso.</summary>
public sealed partial record ChiaveRegistrazione
{
    public ChiaveRegistrazione(string improntaPagina, string versionePrompt, string fornitore, string modello)
    {
        ImprontaPagina = Controlla(improntaPagina?.ToLowerInvariant(), Impronta(), "impronta della pagina");
        VersionePrompt = Controlla(versionePrompt, Versione(), "versione del prompt");
        Fornitore = Controlla(fornitore, FornitoreRegex(), "fornitore");
        Modello = Controlla(modello, ModelloRegex(), "modello");
    }

    /// <summary>SHA-256 della pagina in esadecimale minuscolo.</summary>
    public string ImprontaPagina { get; }

    public string VersionePrompt { get; }

    public string Fornitore { get; }

    public string Modello { get; }

    public override string ToString() => $"{Fornitore}/{Modello}/{VersionePrompt}/{ImprontaPagina}";

    private static string Controlla(string? valore, Regex modello, string nome) =>
        valore is not null && modello.IsMatch(valore)
            ? valore
            : throw new ArgumentException($"Chiave della Registrazione non valida: {nome}.", nome);

    [GeneratedRegex("^[0-9a-f]{64}$")]
    private static partial Regex Impronta();

    [GeneratedRegex("^[a-z0-9][a-z0-9._-]{0,39}$")]
    private static partial Regex Versione();

    [GeneratedRegex("^[a-z][a-z0-9-]{0,39}$")]
    private static partial Regex FornitoreRegex();

    [GeneratedRegex("^[a-z0-9]([a-z0-9._-]{0,98}[a-z0-9])?$")]
    private static partial Regex ModelloRegex();
}

/// <summary>Metadati di una pagina: durata, token e costo, sommati su tutte le chiamate fatte per leggerla.</summary>
public sealed record MetadatiChiamata(
    int DurataMs,
    int? TokenIngresso,
    int? TokenUscita,
    int? TokenCacheCreazione,
    int? TokenCacheLettura,
    decimal? Costo,
    string? CostoValuta,
    string? CostoTipo)
{
    public static MetadatiChiamata Somma(IReadOnlyCollection<ChiamataAi> chiamate)
    {
        var costate = chiamate.Where(c => c.Costo is not null).ToList();
        return new MetadatiChiamata(
            chiamate.Sum(c => c.DurataMs),
            SommaOpzionale(chiamate.Select(c => c.TokenIngresso)),
            SommaOpzionale(chiamate.Select(c => c.TokenUscita)),
            SommaOpzionale(chiamate.Select(c => c.TokenCacheCreazione)),
            SommaOpzionale(chiamate.Select(c => c.TokenCacheLettura)),
            costate.Count == 0 ? null : costate.Sum(c => c.Costo),
            costate.FirstOrDefault()?.CostoValuta,
            costate.FirstOrDefault()?.CostoTipo);
    }

    private static int? SommaOpzionale(IEnumerable<int?> valori)
    {
        var presenti = valori.Where(v => v is not null).ToList();
        return presenti.Count == 0 ? null : presenti.Sum();
    }
}

/// <summary>Una Registrazione: la risposta grezza di una pagina e i metadati della chiamata.</summary>
public sealed record Registrazione(
    ChiaveRegistrazione Chiave,
    DateTimeOffset RegistrataIl,
    int Dpi,
    EsitoEstrazione Esito,
    string RispostaGrezza,
    string RispostaSha256,
    MetadatiChiamata Metadati);

/// <summary>Porta delle Registrazioni su disco: il formato è quello della sezione 6.2 del progetto 1.1.</summary>
public interface IArchivioRegistrazioni
{
    Task SalvaAsync(Registrazione registrazione, CancellationToken cancellationToken = default);

    /// <summary>Restituisce la Registrazione, o <c>null</c> se non esiste; un file illeggibile lancia <see cref="RegistrazioneNonValidaException"/>.</summary>
    Task<Registrazione?> CaricaAsync(ChiaveRegistrazione chiave, CancellationToken cancellationToken = default);
}

/// <summary>Apre l'archivio delle Registrazioni in una cartella.</summary>
public interface IFabbricaArchivioRegistrazioni
{
    IArchivioRegistrazioni Apri(string cartella);
}

/// <summary>Interpreta il testo grezzo di una risposta come farebbe l'adattatore dal vivo; <c>Json</c> è nullo se non contiene un JSON.</summary>
public interface IInterpreteRispostaGrezza
{
    InterpretazioneRisposta Interpreta(string rispostaGrezza);
}

public sealed record InterpretazioneRisposta(string? Json, string? Messaggio);

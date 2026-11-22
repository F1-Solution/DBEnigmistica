using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Caricamento;

/// <summary>Insieme dei descrittori caricati, in ordine di id e di versione.</summary>
public sealed class CatalogoTipologie
{
    private CatalogoTipologie(IReadOnlyList<Descrittore> descrittori)
    {
        Descrittori = descrittori;
    }

    /// <summary>Tutti i descrittori, compresi i prototipi (versione 0) e le versioni vecchie.</summary>
    public IReadOnlyList<Descrittore> Descrittori { get; }

    /// <summary>Crea il catalogo; una coppia (id, versione) ripetuta è un errore.</summary>
    public static CatalogoTipologie Da(IEnumerable<Descrittore> descrittori)
    {
        var ordinati = descrittori.OrderBy(d => d.Id, StringComparer.Ordinal).ThenBy(d => d.Versione).ToList();
        var doppio = ordinati.GroupBy(d => (d.Id, d.Versione)).FirstOrDefault(g => g.Count() > 1);
        if (doppio is not null)
            throw new ArgumentException($"Il descrittore {doppio.Key.Id} v{doppio.Key.Versione} compare più volte.", nameof(descrittori));
        return new CatalogoTipologie(ordinati);
    }

    /// <summary>Per l'estrazione: l'ultima versione di ogni id, esclusi i prototipi, in ordine di id.</summary>
    public IReadOnlyList<Descrittore> PerEstrazione() =>
        [.. Descrittori.Where(d => !d.EPrototipo).GroupBy(d => d.Id).Select(g => g.MaxBy(d => d.Versione)!)];

    /// <summary>Descrittori con SQL da generare: tutte le versioni, esclusi i prototipi.</summary>
    public IReadOnlyList<Descrittore> ConSql() => [.. Descrittori.Where(d => !d.EPrototipo)];
}

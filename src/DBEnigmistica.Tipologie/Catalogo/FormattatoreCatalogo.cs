using System.Globalization;
using System.Text;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Catalogo;

/// <summary>Formato del catalogo per il prompt di estrazione (progetto-1.1, sezione 5.2): testo con a capo LF, senza a capo finale.</summary>
public static class FormattatoreCatalogo
{
    /// <summary>Formatta i descrittori in ordine di id; un prototipo o un id ripetuto è un errore.</summary>
    public static string Formatta(IEnumerable<Descrittore> descrittori)
    {
        ArgumentNullException.ThrowIfNull(descrittori);
        var ordinati = DescrittoriPerCatalogo.Ordina(descrittori);
        return string.Join("\n\n", ordinati.Select(FormattaDescrittore));
    }

    private static string FormattaDescrittore(Descrittore d)
    {
        var testo = new StringBuilder();
        testo.Append(CultureInfo.InvariantCulture, $"### {d.Id} (versione {d.Versione}): {d.Nome}\n");
        testo.Append(CultureInfo.InvariantCulture, $"Famiglia: {NomiModello.Nome(d.Famiglia)}. Resa: {NomiModello.Nome(d.Resa.Modello)}.\n");
        testo.Append(CultureInfo.InvariantCulture, $"Come si riconosce: {d.Descrizione}\n");
        testo.Append(CultureInfo.InvariantCulture, $"Regole: {d.Regole.Testo}\n");
        testo.Append("Campi:");
        foreach (var campo in d.Campi)
            testo.Append('\n').Append(FormattaCampo(campo));

        foreach (var elenco in d.Elenchi)
        {
            testo.Append(CultureInfo.InvariantCulture, $"\nElenco `{elenco.Nome}` ({elenco.Etichetta ?? elenco.Nome}), una voce per riga stampata:");
            foreach (var campo in elenco.Campi)
                testo.Append('\n').Append(FormattaCampo(campo));
        }

        return testo.ToString();
    }

    private static string FormattaCampo(CampoDescrittore c)
    {
        var riga = new StringBuilder();
        riga.Append(CultureInfo.InvariantCulture, $"- `{c.Nome}` ({c.Etichetta ?? c.Nome}): {TipoLeggibile(c)}");
        if (c.Obbligatorio)
            riga.Append(", obbligatorio");
        if (c.Descrizione is not null)
            riga.Append(". ").Append(c.Descrizione);
        return riga.ToString();
    }

    private static string TipoLeggibile(CampoDescrittore c) => c.Tipo switch
    {
        TipoCampo.Testo => $"testo, al massimo {c.Max?.ToString(CultureInfo.InvariantCulture)} caratteri",
        TipoCampo.TestoLungo => "testo lungo",
        TipoCampo.Intero => "numero intero",
        TipoCampo.Decimale => "numero decimale",
        TipoCampo.Booleano => "vero o falso",
        TipoCampo.Data => "data AAAA-MM-GG",
        TipoCampo.Enumerazione => $"uno tra: {string.Join(", ", c.Valori ?? [])}",
        TipoCampo.Griglia => "griglia, nel formato descritto sopra",
        TipoCampo.Immagine => "riquadro dell'immagine",
        _ => throw new ArgumentOutOfRangeException(nameof(c), c.Tipo, "Tipo di campo non previsto."),
    };
}

/// <summary>Ordine e controlli comuni a formattatore e derivazione: per id, nessun prototipo, nessun id ripetuto.</summary>
internal static class DescrittoriPerCatalogo
{
    public static IReadOnlyList<Descrittore> Ordina(IEnumerable<Descrittore> descrittori)
    {
        var ordinati = descrittori.OrderBy(d => d.Id, StringComparer.Ordinal).ToList();
        if (ordinati.Find(d => d.EPrototipo) is { } prototipo)
            throw new ArgumentException($"Il prototipo {prototipo.Id} (versione 0) non entra nel catalogo di estrazione.", nameof(descrittori));
        var doppio = ordinati.GroupBy(d => d.Id).FirstOrDefault(g => g.Count() > 1);
        if (doppio is not null)
            throw new ArgumentException($"La Tipologia {doppio.Key} compare con più versioni: serve l'ultima.", nameof(descrittori));
        return ordinati;
    }
}

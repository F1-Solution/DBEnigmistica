using System.Text;
using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Modello;

namespace DBEnigmistica.Tipologie.Generatore;

/// <summary>
/// Esito del confronto fra l'SQL registrato in <c>tipologie/</c> e quello generato dai descrittori.
/// <c>Scritti</c> sono i file creati o riscritti; <c>Differenti</c> e <c>Mancanti</c> restano compilati solo senza scrittura.
/// </summary>
public sealed record EsitoAllineamento(
    IReadOnlyList<string> Scritti,
    IReadOnlyList<string> Differenti,
    IReadOnlyList<string> Mancanti,
    IReadOnlyList<string> Orfani,
    int Invariati)
{
    /// <summary>True se l'SQL registrato coincide con quello generato e non ci sono file estranei.</summary>
    public bool EAllineato => Differenti.Count == 0 && Mancanti.Count == 0 && Orfani.Count == 0;
}

/// <summary>Scrive o controlla gli SQL <c>tipologie/&lt;id&gt;/v&lt;n&gt;.{riviste,catalogo}.sql</c> (<c>dbe types generate [--check]</c>).</summary>
public static partial class AllineamentoSql
{
    private static readonly UTF8Encoding Utf8SenzaBom = new(false);

    /// <summary>
    /// Confronta (o riscrive, con <paramref name="scrivi"/>) l'SQL generato dai descrittori. Un file SQL senza descrittore con SQL
    /// (per esempio per una versione 0) è orfano e non viene mai cancellato. Il confronto ignora il tipo di a capo del file.
    /// </summary>
    public static EsitoAllineamento Allinea(IEnumerable<Descrittore> descrittori, string cartellaTipologie, bool scrivi)
    {
        ArgumentNullException.ThrowIfNull(descrittori);
        var attesi = GeneratoreSql.GeneraTutti(descrittori);
        var scritti = new List<string>();
        var differenti = new List<string>();
        var mancanti = new List<string>();
        var invariati = 0;

        foreach (var file in attesi)
        {
            var percorso = Path.Combine(cartellaTipologie, file.Percorso.Replace('/', Path.DirectorySeparatorChar));
            var presente = File.Exists(percorso);
            if (presente && ConLf(File.ReadAllText(percorso, Utf8SenzaBom)) == file.Contenuto)
            {
                invariati++;
                continue;
            }

            if (scrivi)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(percorso)!);
                File.WriteAllText(percorso, file.Contenuto, Utf8SenzaBom);
                scritti.Add(file.Percorso);
            }
            else
            {
                (presente ? differenti : mancanti).Add(file.Percorso);
            }
        }

        return new EsitoAllineamento(scritti, differenti, mancanti, Orfani(cartellaTipologie, attesi), invariati);
    }

    private static List<string> Orfani(string cartellaTipologie, IReadOnlyList<FileSql> attesi)
    {
        var previsti = attesi.Select(f => f.Percorso).ToHashSet(StringComparer.Ordinal);
        var orfani = new List<string>();
        foreach (var cartella in Directory.EnumerateDirectories(cartellaTipologie).Where(d => !Path.GetFileName(d).StartsWith('_')).Order(StringComparer.Ordinal))
        {
            foreach (var file in Directory.EnumerateFiles(cartella, "v*.sql").Order(StringComparer.Ordinal))
            {
                var relativo = $"{Path.GetFileName(cartella)}/{Path.GetFileName(file)}";
                if (NomeSql().IsMatch(Path.GetFileName(file)) && !previsti.Contains(relativo))
                    orfani.Add(relativo);
            }
        }

        return orfani;
    }

    private static string ConLf(string testo) => testo.Replace("\r\n", "\n", StringComparison.Ordinal);

    [GeneratedRegex(@"^v[0-9]{1,3}\.(riviste|catalogo)\.sql\z")]
    private static partial Regex NomeSql();
}

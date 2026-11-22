using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using DBEnigmistica.Tipologie.Modello;
using DBEnigmistica.Tipologie.Validatori;

namespace DBEnigmistica.Tipologie.Caricamento;

/// <summary>
/// Carica i descrittori: validazione contro il meta-schema, poi i controlli della sezione 2.4 di progetto-1.1.
/// Accetta la versione 0 (prototipo). Il controllo 6 (versione già registrata con colonne diverse) lo fa <c>types generate</c> sull'SQL registrato.
/// </summary>
public sealed partial class CaricatoreDescrittori
{
    private const int MassimoByteIdentificatore = 63;
    private const string PrefissoTabella = "gioco_";

    private static readonly ValidatoreSchema Schemi = new();
    private static readonly Lazy<string> MetaSchema = new(() => RisorseTipologie.Leggi("_meta/descrittore.schema.json"));

    private readonly IReadOnlyDictionary<NomeValidatore, IValidatoreStrutturale> _validatori;

    /// <param name="validatori">Implementazioni per il controllo 5; se null si usa <see cref="CatalogoValidatori.Tutti"/>.</param>
    public CaricatoreDescrittori(IReadOnlyCollection<IValidatoreStrutturale>? validatori = null)
    {
        _validatori = (validatori ?? CatalogoValidatori.Tutti).ToDictionary(v => v.Nome);
    }

    /// <summary>Carica un testo JSON senza controllare nome della cartella e del file (controllo 3).</summary>
    public Descrittore CaricaTesto(string json, string origine = "descrittore") => Carica(json, origine, null, null);

    /// <summary>Carica <c>tipologie/&lt;id&gt;/v&lt;n&gt;.json</c>: l'id deve essere uguale alla cartella e la versione al nome del file.</summary>
    public Descrittore CaricaFile(string percorso)
    {
        var cartella = new FileInfo(percorso).Directory?.Name ?? string.Empty;
        return CaricaConNomi(File.ReadAllText(percorso, Encoding.UTF8), percorso, cartella, Path.GetFileName(percorso));
    }

    /// <summary>Carica ogni <c>tipologie/*/v*.json</c> di una cartella, comprese le versioni 0.</summary>
    public CatalogoTipologie CaricaCartella(string cartellaTipologie)
    {
        var descrittori = Directory.EnumerateDirectories(cartellaTipologie)
            .Where(d => !Path.GetFileName(d).StartsWith('_'))
            .SelectMany(d => Directory.EnumerateFiles(d, "v*.json"))
            .Where(f => NomeFile().IsMatch(Path.GetFileName(f)))
            .Order(StringComparer.Ordinal)
            .Select(CaricaFile);
        return CatalogoTipologie.Da(descrittori);
    }

    /// <summary>Carica i descrittori incorporati nell'assembly (stessi file di <c>tipologie/</c>).</summary>
    public CatalogoTipologie CaricaRisorse()
    {
        var descrittori = RisorseTipologie.Elenca(".json")
            .Where(p => !p.StartsWith('_') && p.Contains('/'))
            .Select(p => (Percorso: p, Cartella: p[..p.IndexOf('/')], File: p[(p.IndexOf('/') + 1)..]))
            .Where(r => NomeFile().IsMatch(r.File))
            .Select(r => CaricaConNomi(RisorseTipologie.Leggi(r.Percorso), $"risorsa {r.Percorso}", r.Cartella, r.File));
        return CatalogoTipologie.Da(descrittori);
    }

    private Descrittore CaricaConNomi(string json, string origine, string cartella, string nomeFile)
    {
        var corrispondenza = NomeFile().Match(nomeFile);
        var versione = corrispondenza.Success ? int.Parse(corrispondenza.Groups["n"].Value, System.Globalization.CultureInfo.InvariantCulture) : -1;
        return Carica(json, origine, cartella, versione, nomeFile);
    }

    private Descrittore Carica(string json, string origine, string? cartella, int? versioneFile, string? nomeFile = null)
    {
        var errori = Schemi.Valida(json, MetaSchema.Value).ToList();
        if (errori.Count > 0)
            throw new DescrittoreNonValidoException(origine, errori);

        using var documento = JsonDocument.Parse(json);
        var descrittore = LettoreDescrittore.Leggi(documento.RootElement);

        errori.AddRange(NomiUnici(descrittore));
        errori.AddRange(RuoliUnici(descrittore));
        if (cartella is not null)
            errori.AddRange(NomiDelFile(descrittore, cartella, versioneFile, nomeFile));
        errori.AddRange(LunghezzaIdentificatori(descrittore));
        errori.AddRange(ValidatoriApplicabili(descrittore));

        return errori.Count == 0 ? descrittore : throw new DescrittoreNonValidoException(origine, errori);
    }

    // Controllo 1.
    private static IEnumerable<string> NomiUnici(Descrittore d)
    {
        foreach (var nome in Duplicati(d.Campi.Select(c => c.Nome).Concat(d.Elenchi.Select(e => e.Nome))))
            yield return $"Il nome '{nome}' compare più volte fra i campi del Gioco e gli elenchi.";

        foreach (var elenco in d.Elenchi)
            foreach (var nome in Duplicati(elenco.Campi.Select(c => c.Nome)))
                yield return $"Il nome '{nome}' compare più volte nei campi dell'elenco '{elenco.Nome}'.";
    }

    // Controllo 2.
    private static IEnumerable<string> RuoliUnici(Descrittore d)
    {
        foreach (var ruolo in Duplicati(d.Campi.Where(c => c.Ruolo is not null).Select(c => NomiModello.Nome(c.Ruolo!.Value))))
            yield return $"Il ruolo '{ruolo}' compare più volte nei campi del Gioco.";

        foreach (var elenco in d.Elenchi)
            foreach (var ruolo in Duplicati(elenco.Campi.Where(c => c.Ruolo is not null).Select(c => NomiModello.Nome(c.Ruolo!.Value))))
                yield return $"Il ruolo '{ruolo}' compare più volte nell'elenco '{elenco.Nome}'.";
    }

    // Controllo 3.
    private static IEnumerable<string> NomiDelFile(Descrittore d, string cartella, int? versioneFile, string? nomeFile)
    {
        if (!string.Equals(d.Id, cartella, StringComparison.Ordinal))
            yield return $"L'id '{d.Id}' deve essere uguale al nome della cartella '{cartella}'.";

        if (versioneFile is null or < 0)
            yield return $"Il nome del file '{nomeFile}' deve essere v<versione>.json.";
        else if (versioneFile != d.Versione)
            yield return $"La versione {d.Versione} deve essere uguale a quella del nome del file ({nomeFile}).";
    }

    // Controllo 4: nome della tabella dell'elenco (la più lunga); i vincoli si accorciano nel generatore.
    private static IEnumerable<string> LunghezzaIdentificatori(Descrittore d)
    {
        var @base = PrefissoTabella + d.Id.Replace('-', '_');
        foreach (var nome in new[] { @base }.Concat(d.Elenchi.Select(e => $"{@base}__{e.Nome}")))
            if (Encoding.UTF8.GetByteCount(nome) > MassimoByteIdentificatore)
                yield return $"Il nome di tabella '{nome}' supera {MassimoByteIdentificatore} byte.";
    }

    // Controllo 5.
    private IEnumerable<string> ValidatoriApplicabili(Descrittore d)
    {
        var ruoliPresenti = d.Campi.Concat(d.Elenchi.SelectMany(e => e.Campi)).Where(c => c.Ruolo is not null).Select(c => c.Ruolo!.Value).ToHashSet();
        foreach (var nome in d.Validatori)
        {
            if (!_validatori.TryGetValue(nome, out var validatore))
            {
                yield return $"Il validatore '{NomiModello.Nome(nome)}' non ha un'implementazione.";
                continue;
            }

            foreach (var ruolo in validatore.RuoliRichiesti.Where(r => !ruoliPresenti.Contains(r)))
                yield return $"Il validatore '{NomiModello.Nome(nome)}' richiede il ruolo '{NomiModello.Nome(ruolo)}', assente nel descrittore.";
        }
    }

    private static IEnumerable<string> Duplicati(IEnumerable<string> nomi) =>
        nomi.GroupBy(n => n, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key);

    [GeneratedRegex(@"^v(?<n>[0-9]{1,3})\.json$")]
    private static partial Regex NomeFile();
}

using System.Text.RegularExpressions;
using DBEnigmistica.Application.Creazione;

namespace DBEnigmistica.Infrastructure.Prototipo;

/// <summary>Descrittori letti da <c>&lt;cartella&gt;/&lt;id&gt;/v&lt;versione&gt;.json</c>: le regole le approva il proprietario nel file.</summary>
public sealed partial class CatalogoTipologieFileSystem(string cartella) : ICatalogoTipologie
{
    public async Task<DescrittoreTipologia?> TrovaAsync(string id, int versione, CancellationToken cancellationToken = default)
    {
        // L'id diventa un percorso: solo la forma dei descrittori, mai separatori.
        if (id is null || !FormaId().IsMatch(id) || versione is < 0 or > 999)
            return null;

        var percorso = Path.Combine(cartella, id, $"v{versione}.json");
        if (!File.Exists(percorso))
            return null;

        var descrittore = DescrittoreTipologia.Leggi(await File.ReadAllTextAsync(percorso, cancellationToken));
        return descrittore.Id == id && descrittore.Versione == versione
            ? descrittore
            : throw new FormatException($"Il file {id}/v{versione}.json descrive un'altra Tipologia o versione.");
    }

    public async Task<IReadOnlyList<DescrittoreTipologia>> ElencaAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(cartella))
            return [];

        var trovati = new List<DescrittoreTipologia>();
        foreach (var cartellaId in Directory.EnumerateDirectories(cartella).Order(StringComparer.Ordinal))
        {
            var id = Path.GetFileName(cartellaId);
            if (!FormaId().IsMatch(id))
                continue;

            foreach (var file in Directory.EnumerateFiles(cartellaId, "v*.json"))
            {
                var corrispondenza = FormaFile().Match(Path.GetFileName(file));
                if (!corrispondenza.Success)
                    continue;

                var versione = int.Parse(corrispondenza.Groups["n"].Value, System.Globalization.CultureInfo.InvariantCulture);
                if (await TrovaAsync(id, versione, cancellationToken) is { } descrittore)
                    trovati.Add(descrittore);
            }
        }

        return [.. trovati.OrderBy(d => d.Id, StringComparer.Ordinal).ThenBy(d => d.Versione)];
    }

    [GeneratedRegex(@"^v(?<n>[0-9]{1,3})\.json$")]
    private static partial Regex FormaFile();

    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)*$")]
    private static partial Regex FormaId();
}

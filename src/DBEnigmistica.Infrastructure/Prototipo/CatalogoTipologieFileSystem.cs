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

    [GeneratedRegex("^[a-z][a-z0-9]*(-[a-z0-9]+)*$")]
    private static partial Regex FormaId();
}

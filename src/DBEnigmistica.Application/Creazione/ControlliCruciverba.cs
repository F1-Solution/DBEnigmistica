using System.Text.Json;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Controllo dopo lo schema: le lettere di ogni parola nella griglia coincidono con la soluzione della sua definizione.</summary>
public static class ControlliCruciverba
{
    public static IReadOnlyList<string> Verifica(DescrittoreTipologia descrittore, SchemaCaselle caselle, string contenutoJson)
    {
        ArgumentNullException.ThrowIfNull(descrittore);
        ArgumentNullException.ThrowIfNull(caselle);
        var errori = new List<string>();
        using var documento = JsonDocument.Parse(contenutoJson);
        var dati = documento.RootElement.GetProperty("dati");

        var nomeGriglia = descrittore.Campi.FirstOrDefault(c => c.Ruolo == "griglia")?.Nome;
        var elenco = descrittore.Elenchi.FirstOrDefault(e => e.Campi.Any(c => c.Ruolo == "soluzione") && e.Campi.Any(c => c.Ruolo == "numero"));
        if (nomeGriglia is null || elenco is null || !dati.TryGetProperty(nomeGriglia, out var griglia))
            return errori;

        var righe = griglia.GetProperty("celle").EnumerateArray().Select(r => r.GetString() ?? string.Empty).ToList();
        var nomeNumero = elenco.Campi.First(c => c.Ruolo == "numero").Nome;
        var nomeDirezione = elenco.Campi.First(c => c.Ruolo == "direzione").Nome;
        var nomeSoluzione = elenco.Campi.First(c => c.Ruolo == "soluzione").Nome;

        foreach (var voce in dati.GetProperty(elenco.Nome).EnumerateArray())
        {
            var numero = voce.GetProperty(nomeNumero).GetInt32();
            var direzione = voce.GetProperty(nomeDirezione).GetString();
            var soluzione = voce.GetProperty(nomeSoluzione).GetString() ?? string.Empty;
            var parola = caselle.Parole.FirstOrDefault(p => p.Numero == numero && p.Direzione == direzione);
            if (parola is null)
            {
                errori.Add($"La definizione {numero} {direzione} non corrisponde a una parola dello schema.");
                continue;
            }

            var lette = Lettere(righe, parola);
            if (!string.Equals(lette, soluzione, StringComparison.Ordinal))
                errori.Add($"Le lettere in griglia della parola {numero} {direzione} ({lette}) non coincidono con la soluzione ({soluzione}).");
        }

        return errori;
    }

    private static string Lettere(IReadOnlyList<string> righe, ParolaSchema parola)
    {
        var testo = new char[parola.Lunghezza];
        for (var i = 0; i < parola.Lunghezza; i++)
        {
            var riga = parola.Riga - 1 + (parola.Direzione == SchemaCaselle.Verticale ? i : 0);
            var colonna = parola.Colonna - 1 + (parola.Direzione == SchemaCaselle.Orizzontale ? i : 0);
            testo[i] = riga < righe.Count && colonna < righe[riga].Length ? righe[riga][colonna] : '?';
        }

        return new string(testo);
    }
}

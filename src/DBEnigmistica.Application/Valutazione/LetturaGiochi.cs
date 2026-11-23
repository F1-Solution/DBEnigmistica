using System.Text.Json;

namespace DBEnigmistica.Application.Valutazione;

/// <summary>Risultato atteso di una pagina (progetto 1.1, sezione 4.1), con i soli campi che le metriche di questa versione usano.</summary>
public sealed record RisultatoAtteso(string Id, string Impronta, string Origine, bool InBozza, IReadOnlyList<GiocoPagina> Giochi);

/// <summary>Legge i Giochi di un file atteso e di una risposta del Fornitore AI.</summary>
public static class LetturaGiochi
{
    public static RisultatoAtteso LeggiAtteso(string percorso, string idAtteso, string origineAttesa)
    {
        if (!File.Exists(percorso))
            throw new SetNonValidoException($"Pagina {idAtteso}: manca il risultato atteso ({Path.GetFileName(percorso)}).");

        try
        {
            using var documento = JsonDocument.Parse(File.ReadAllText(percorso));
            var radice = documento.RootElement;
            var pagina = radice.GetProperty("pagina");
            var id = pagina.GetProperty("id").GetString()!;
            var origine = pagina.GetProperty("origine").GetString()!;
            if (id != idAtteso || origine != origineAttesa)
                throw new SetNonValidoException($"Pagina {idAtteso}: id o origine del risultato atteso non coincidono con set.json.");

            var bozza = radice.GetProperty("revisione").GetProperty("stato").GetString() == "bozza";
            return new RisultatoAtteso(id, pagina.GetProperty("impronta").GetString()!, origine, bozza, LeggiElenco(radice.GetProperty("giochi")));
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException and not ValutazioneException or FormatException)
        {
            throw new SetNonValidoException($"Pagina {idAtteso}: il risultato atteso non è leggibile ({ex.GetType().Name}).", ex);
        }
    }

    /// <summary>I Giochi della risposta del Fornitore AI, già validata contro lo schema.</summary>
    public static IReadOnlyList<GiocoPagina> LeggiRisposta(string json)
    {
        using var documento = JsonDocument.Parse(json);
        return LeggiElenco(documento.RootElement.GetProperty("giochi"));
    }

    private static List<GiocoPagina> LeggiElenco(JsonElement giochi)
    {
        var risultato = new List<GiocoPagina>();
        foreach (var gioco in giochi.EnumerateArray())
        {
            var chiave = gioco.GetProperty("stato_tipologia").GetString() switch
            {
                "nota" => "nota:" + gioco.GetProperty("tipologia").GetString(),
                "proposta" => "proposta",
                var altro => throw new FormatException($"stato_tipologia {altro}")
            };
            var r = gioco.GetProperty("riquadro");
            JsonElement? dati = gioco.TryGetProperty("dati", out var d) && d.ValueKind == JsonValueKind.Object ? d.Clone() : null;
            var nonValutati = gioco.TryGetProperty("campi_non_valutati", out var n) && n.ValueKind == JsonValueKind.Array
                ? n.EnumerateArray().Select(v => v.GetString()!).ToList()
                : [];
            risultato.Add(new GiocoPagina(
                chiave,
                new RiquadroGioco(
                    r.GetProperty("x").GetDouble(), r.GetProperty("y").GetDouble(),
                    r.GetProperty("larghezza").GetDouble(), r.GetProperty("altezza").GetDouble()),
                dati,
                nonValutati));
        }

        return risultato;
    }
}

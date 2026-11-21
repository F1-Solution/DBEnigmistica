using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DBEnigmistica.Application.Creazione;

/// <summary>Differenze tra contenuto generato e finale, su <c>/titolo</c> e <c>/dati/**</c> (progetto 1b.1, sezione 4.3).</summary>
public static class Differenze
{
    private static readonly JsonSerializerOptions Compatto = new() { WriteIndented = false, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    /// <summary>Restituisce il JSON dell'array di differenze e il loro numero.</summary>
    public static (string Json, int Numero) Calcola(string generato, string finale)
    {
        var a = JsonNode.Parse(generato)!;
        var b = JsonNode.Parse(finale)!;
        var voci = new JsonArray();
        Confronta("/titolo", a["titolo"], b["titolo"], voci);
        Confronta("/dati", a["dati"], b["dati"], voci);
        return (voci.ToJsonString(Compatto), voci.Count);
    }

    private static void Confronta(string percorso, JsonNode? prima, JsonNode? dopo, JsonArray voci)
    {
        switch (prima, dopo)
        {
            case (JsonObject oa, JsonObject ob):
                foreach (var (chiave, valore) in oa)
                    Confronta($"{percorso}/{Escape(chiave)}", valore, ob.TryGetPropertyValue(chiave, out var altro) ? altro : null, voci);
                foreach (var (chiave, valore) in ob.Where(p => !oa.ContainsKey(p.Key)))
                    Confronta($"{percorso}/{Escape(chiave)}", null, valore, voci);
                break;
            case (JsonArray aa, JsonArray ab):
                for (var i = 0; i < Math.Max(aa.Count, ab.Count); i++)
                {
                    Confronta(
                        $"{percorso}/{i.ToString(CultureInfo.InvariantCulture)}",
                        i < aa.Count ? aa[i] : null,
                        i < ab.Count ? ab[i] : null,
                        voci,
                        voceIntera: i >= aa.Count || i >= ab.Count);
                }

                break;
            default:
                if (!JsonNode.DeepEquals(prima, dopo))
                    voci.Add(Voce(percorso, prima, dopo));
                break;
        }
    }

    // Una voce di un elenco in più o in meno si riporta intera, con null dall'altra parte.
    private static void Confronta(string percorso, JsonNode? prima, JsonNode? dopo, JsonArray voci, bool voceIntera)
    {
        if (voceIntera)
            voci.Add(Voce(percorso, prima, dopo));
        else
            Confronta(percorso, prima, dopo, voci);
    }

    private static JsonObject Voce(string percorso, JsonNode? prima, JsonNode? dopo) => new()
    {
        ["percorso"] = percorso,
        ["prima"] = prima?.DeepClone(),
        ["dopo"] = dopo?.DeepClone(),
    };

    private static string Escape(string chiave) => chiave.Replace("~", "~0", StringComparison.Ordinal).Replace("/", "~1", StringComparison.Ordinal);
}

/// <summary>Serializzazione canonica RFC 8785 (JCS) e il suo SHA-256: per collegare l'esito alla versione salvata senza testo nel registro.</summary>
public static class Jcs
{
    public static string Sha256Esadecimale(string json) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(Canonicalizza(json))));

    public static string Canonicalizza(string json)
    {
        using var documento = JsonDocument.Parse(json);
        var testo = new StringBuilder();
        Scrivi(documento.RootElement, testo);
        return testo.ToString();
    }

    private static void Scrivi(JsonElement e, StringBuilder testo)
    {
        switch (e.ValueKind)
        {
            case JsonValueKind.Object:
                testo.Append('{');
                var primo = true;
                foreach (var proprieta in e.EnumerateObject().OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    if (!primo)
                        testo.Append(',');
                    primo = false;
                    Stringa(proprieta.Name, testo);
                    testo.Append(':');
                    Scrivi(proprieta.Value, testo);
                }

                testo.Append('}');
                break;
            case JsonValueKind.Array:
                testo.Append('[');
                var prima = true;
                foreach (var elemento in e.EnumerateArray())
                {
                    if (!prima)
                        testo.Append(',');
                    prima = false;
                    Scrivi(elemento, testo);
                }

                testo.Append(']');
                break;
            case JsonValueKind.String:
                Stringa(e.GetString()!, testo);
                break;
            case JsonValueKind.Number:
                testo.Append(Numero(e));
                break;
            case JsonValueKind.True:
                testo.Append("true");
                break;
            case JsonValueKind.False:
                testo.Append("false");
                break;
            default:
                testo.Append("null");
                break;
        }
    }

    // Numeri interi come interi; gli altri con la rappresentazione più breve del double, senza notazione esponenziale per i valori comuni.
    private static string Numero(JsonElement e)
    {
        if (e.TryGetInt64(out var intero))
            return intero.ToString(CultureInfo.InvariantCulture);

        var valore = e.GetDouble();
        return valore == Math.Floor(valore) && Math.Abs(valore) < 1e21
            ? valore.ToString("F0", CultureInfo.InvariantCulture)
            : valore.ToString("R", CultureInfo.InvariantCulture);
    }

    private static void Stringa(string valore, StringBuilder testo)
    {
        testo.Append('"');
        foreach (var c in valore)
        {
            switch (c)
            {
                case '"': testo.Append("\\\""); break;
                case '\\': testo.Append("\\\\"); break;
                case '\b': testo.Append("\\b"); break;
                case '\t': testo.Append("\\t"); break;
                case '\n': testo.Append("\\n"); break;
                case '\f': testo.Append("\\f"); break;
                case '\r': testo.Append("\\r"); break;
                case < ' ': testo.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture)); break;
                default: testo.Append(c); break;
            }
        }

        testo.Append('"');
    }
}

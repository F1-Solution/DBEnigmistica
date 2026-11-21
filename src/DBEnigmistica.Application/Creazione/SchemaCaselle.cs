using System.Text.Encodings.Web;
using System.Text.Json;

namespace DBEnigmistica.Application.Creazione;

public sealed record NumeroCasella(int Riga, int Colonna, int Numero);

public sealed record ParolaSchema(int Numero, string Direzione, int Riga, int Colonna, int Lunghezza);

/// <summary>Schema delle caselle del cruciverba (progetto 1b.1, sezione 5.4): lo produce il generatore, lo usano prompt e schema.</summary>
public sealed record SchemaCaselle(
    int Righe,
    int Colonne,
    IReadOnlyList<string> Celle,
    IReadOnlyList<NumeroCasella> Numeri,
    IReadOnlyList<ParolaSchema> Parole,
    long Seme,
    string VersioneGeneratore)
{
    public const string Orizzontale = "orizzontale";
    public const string Verticale = "verticale";

    public static SchemaCaselle Leggi(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using var documento = JsonDocument.Parse(json);
            var r = documento.RootElement;
            return new SchemaCaselle(
                r.GetProperty("righe").GetInt32(),
                r.GetProperty("colonne").GetInt32(),
                [.. r.GetProperty("celle").EnumerateArray().Select(x => x.GetString()!)],
                [.. r.GetProperty("numeri").EnumerateArray().Select(n => new NumeroCasella(
                    n.GetProperty("riga").GetInt32(), n.GetProperty("colonna").GetInt32(), n.GetProperty("numero").GetInt32()))],
                [.. r.GetProperty("parole").EnumerateArray().Select(p => new ParolaSchema(
                    p.GetProperty("numero").GetInt32(), p.GetProperty("direzione").GetString()!,
                    p.GetProperty("riga").GetInt32(), p.GetProperty("colonna").GetInt32(), p.GetProperty("lunghezza").GetInt32()))],
                r.GetProperty("seme").GetInt64(),
                r.GetProperty("versione_generatore").GetString()!);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new FormatException($"Schema delle caselle non leggibile: {ex.Message}", ex);
        }
    }

    public string ToJson()
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, new JsonWriterOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            w.WriteNumber("righe", Righe);
            w.WriteNumber("colonne", Colonne);
            w.WriteStartArray("celle");
            foreach (var riga in Celle)
                w.WriteStringValue(riga);
            w.WriteEndArray();
            w.WriteStartArray("numeri");
            foreach (var n in Numeri)
            {
                w.WriteStartObject();
                w.WriteNumber("riga", n.Riga);
                w.WriteNumber("colonna", n.Colonna);
                w.WriteNumber("numero", n.Numero);
                w.WriteEndObject();
            }

            w.WriteEndArray();
            w.WriteStartArray("parole");
            foreach (var p in Parole)
            {
                w.WriteStartObject();
                w.WriteNumber("numero", p.Numero);
                w.WriteString("direzione", p.Direzione);
                w.WriteNumber("riga", p.Riga);
                w.WriteNumber("colonna", p.Colonna);
                w.WriteNumber("lunghezza", p.Lunghezza);
                w.WriteEndObject();
            }

            w.WriteEndArray();
            w.WriteNumber("seme", Seme);
            w.WriteString("versione_generatore", VersioneGeneratore);
            w.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(memoria.ToArray());
    }
}

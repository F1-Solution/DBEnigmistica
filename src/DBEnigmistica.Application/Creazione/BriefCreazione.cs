using System.Text.Encodings.Web;
using System.Text.Json;

namespace DBEnigmistica.Application.Creazione;

public sealed record DimensioniBrief(int Righe, int Colonne);

/// <summary>Brief di un candidato (progetto 1b.1, sezione 2): immutabile, con la variante scritta dal programma.</summary>
public sealed record BriefCreazione(
    string? Codice,
    string Tipologia,
    int VersioneTipologia,
    string Famiglia,
    string? Tema,
    IReadOnlyList<string> Vincoli,
    int Difficolta,
    DimensioniBrief? Dimensioni,
    string? Sottotipo,
    int? Variante)
{
    private static readonly JsonWriterOptions OpzioniScrittura = new()
    {
        Indented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public BriefCreazione ConVariante(int variante) => this with { Variante = variante };

    /// <summary>Legge un brief già validato dallo schema; l'eventuale <c>$schema</c> si ignora.</summary>
    public static BriefCreazione Leggi(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using var documento = JsonDocument.Parse(json);
            var r = documento.RootElement;
            DimensioniBrief? dimensioni = null;
            if (r.GetProperty("dimensioni") is { ValueKind: JsonValueKind.Object } d)
                dimensioni = new DimensioniBrief(d.GetProperty("righe").GetInt32(), d.GetProperty("colonne").GetInt32());

            return new BriefCreazione(
                Testo(r, "codice"),
                r.GetProperty("tipologia").GetString()!,
                r.GetProperty("versione_tipologia").GetInt32(),
                r.GetProperty("famiglia").GetString()!,
                Testo(r, "tema"),
                [.. r.GetProperty("vincoli").EnumerateArray().Select(x => x.GetString()!)],
                r.GetProperty("difficolta").GetInt32(),
                dimensioni,
                Testo(r, "sottotipo"),
                r.TryGetProperty("variante", out var v) ? v.GetInt32() : null);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new FormatException($"Brief non leggibile: {ex.Message}", ex);
        }
    }

    /// <summary>JSON compatto con le chiavi nell'ordine del contratto; è il testo salvato e quello della richiesta.</summary>
    public string ToJson()
    {
        using var memoria = new MemoryStream();
        using (var w = new Utf8JsonWriter(memoria, OpzioniScrittura))
        {
            w.WriteStartObject();
            w.WriteNumber("versione_formato", 1);
            if (Codice is not null)
                w.WriteString("codice", Codice);
            w.WriteString("tipologia", Tipologia);
            w.WriteNumber("versione_tipologia", VersioneTipologia);
            w.WriteString("famiglia", Famiglia);
            Scrivi(w, "tema", Tema);
            w.WriteStartArray("vincoli");
            foreach (var vincolo in Vincoli)
                w.WriteStringValue(vincolo);
            w.WriteEndArray();
            w.WriteNumber("difficolta", Difficolta);
            if (Dimensioni is { } d)
            {
                w.WriteStartObject("dimensioni");
                w.WriteNumber("righe", d.Righe);
                w.WriteNumber("colonne", d.Colonne);
                w.WriteEndObject();
            }
            else
            {
                w.WriteNull("dimensioni");
            }

            Scrivi(w, "sottotipo", Sottotipo);
            if (Variante is { } variante)
                w.WriteNumber("variante", variante);
            w.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(memoria.ToArray());
    }

    /// <summary>Chiave di confronto del brief senza la variante: due brief uguali per tutto il resto hanno la stessa.</summary>
    public string SenzaVarianteJson() => ConVarianteAssente().ToJson();

    private BriefCreazione ConVarianteAssente() => this with { Variante = null };

    private static void Scrivi(Utf8JsonWriter w, string nome, string? valore)
    {
        if (valore is null)
            w.WriteNull(nome);
        else
            w.WriteString(nome, valore);
    }

    private static string? Testo(JsonElement e, string nome) =>
        e.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}

using System.Text.Json;

namespace DBEnigmistica.Application.Creazione;

public sealed record CampoDescrittore(
    string Nome,
    string? Etichetta,
    string? Descrizione,
    string Tipo,
    bool Obbligatorio,
    int? Max,
    IReadOnlyList<string> Valori,
    string? Ruolo)
{
    public string EtichettaONome => Etichetta ?? Nome;
}

public sealed record ElencoDescrittore(string Nome, string? Etichetta, string? Descrizione, IReadOnlyList<CampoDescrittore> Campi)
{
    public string EtichettaONome => Etichetta ?? Nome;
}

public sealed record RegoleDescrittore(string Testo, int Versione, bool Approvata, string? ApprovateIl);

/// <summary>Descrittore di una Tipologia, nella parte che serve alla creazione (progetto 1.1, sezione 2).</summary>
public sealed record DescrittoreTipologia(
    string Id,
    int Versione,
    string Famiglia,
    string Nome,
    string Descrizione,
    RegoleDescrittore Regole,
    IReadOnlyList<CampoDescrittore> Campi,
    IReadOnlyList<ElencoDescrittore> Elenchi,
    string Resa)
{
    public const string TipoImmagine = "immagine";

    /// <summary>Legge un descrittore da JSON; un file che non ha la forma attesa lancia <see cref="FormatException"/>.</summary>
    public static DescrittoreTipologia Leggi(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using var documento = JsonDocument.Parse(json);
            var r = documento.RootElement;
            var regole = r.GetProperty("regole");
            return new DescrittoreTipologia(
                r.GetProperty("id").GetString()!,
                r.GetProperty("versione").GetInt32(),
                r.GetProperty("famiglia").GetString()!,
                r.GetProperty("nome").GetString()!,
                r.GetProperty("descrizione").GetString()!,
                new RegoleDescrittore(
                    regole.GetProperty("testo").GetString()!,
                    regole.GetProperty("versione").GetInt32(),
                    regole.GetProperty("stato").GetString() == "approvata",
                    Testo(regole, "approvate_il")),
                [.. r.GetProperty("campi").EnumerateArray().Select(Campo)],
                [.. r.GetProperty("elenchi").EnumerateArray().Select(Elenco)],
                r.GetProperty("resa").GetProperty("modello").GetString()!);
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException or InvalidOperationException or FormatException)
        {
            throw new FormatException($"Descrittore non leggibile: {ex.Message}", ex);
        }
    }

    private static CampoDescrittore Campo(JsonElement e) => new(
        e.GetProperty("nome").GetString()!,
        Testo(e, "etichetta"),
        Testo(e, "descrizione"),
        e.GetProperty("tipo").GetString()!,
        e.TryGetProperty("obbligatorio", out var o) && o.ValueKind == JsonValueKind.True,
        e.TryGetProperty("max", out var m) ? m.GetInt32() : null,
        e.TryGetProperty("valori", out var v) ? [.. v.EnumerateArray().Select(x => x.GetString()!)] : [],
        Testo(e, "ruolo"));

    private static ElencoDescrittore Elenco(JsonElement e) => new(
        e.GetProperty("nome").GetString()!,
        Testo(e, "etichetta"),
        Testo(e, "descrizione"),
        [.. e.GetProperty("campi").EnumerateArray().Select(Campo)]);

    private static string? Testo(JsonElement e, string nome) =>
        e.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
}

using System.Text.Json;

namespace DBEnigmistica.Tipologie.Validatori;

/// <summary>Letture tolleranti dei dati di un Gioco: una proprietà assente o null vale "non presente".</summary>
internal static class LetturaDati
{
    public static string? Testo(JsonElement oggetto, string nome) =>
        oggetto.ValueKind == JsonValueKind.Object && oggetto.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString()
            : null;

    public static int? Intero(JsonElement oggetto, string nome) =>
        oggetto.ValueKind == JsonValueKind.Object && oggetto.TryGetProperty(nome, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out var n)
            ? n
            : null;

    public static JsonElement? Elemento(JsonElement oggetto, string nome) =>
        oggetto.ValueKind == JsonValueKind.Object && oggetto.TryGetProperty(nome, out var v) && v.ValueKind != JsonValueKind.Null
            ? v
            : null;
}

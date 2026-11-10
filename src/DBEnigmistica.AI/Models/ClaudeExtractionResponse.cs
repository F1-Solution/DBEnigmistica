using System.Text.Json.Serialization;
using System.Text.Json;

namespace DBEnigmistica.AI.Models;

/// <summary>
/// Risposta dell'AI per l'estrazione dati da un gioco
/// </summary>
public class ClaudeExtractionResponse
{
    [JsonPropertyName("gameType")]
    public required string GameType { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }

    [JsonPropertyName("fields")]
    public Dictionary<string, JsonElement> Fields { get; set; } = new();

    [JsonPropertyName("warnings")]
    public List<string> Warnings { get; set; } = new();
}

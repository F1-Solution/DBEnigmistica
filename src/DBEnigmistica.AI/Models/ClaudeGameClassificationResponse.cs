using System.Text.Json.Serialization;

namespace DBEnigmistica.AI.Models;

/// <summary>
/// Risposta dell'AI per la classificazione di un gioco
/// </summary>
public class ClaudeGameClassificationResponse
{
    [JsonPropertyName("gameType")]
    public required string GameType { get; set; }

    [JsonPropertyName("gameTitle")]
    public required string GameTitle { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}

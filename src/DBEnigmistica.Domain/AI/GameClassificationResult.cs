namespace DBEnigmistica.Domain.AI;

/// <summary>
/// Risultato della classificazione di un gioco da parte dell'AI
/// </summary>
public class GameClassificationResult
{
    public required string GameType { get; init; }
    public required string GameTitle { get; init; }
    public required float Confidence { get; init; }
    public string? Description { get; init; }
    public Dictionary<string, string> Metadata { get; init; } = new();
}

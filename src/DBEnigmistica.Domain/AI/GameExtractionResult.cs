namespace DBEnigmistica.Domain.AI;

/// <summary>
/// Dati estratti da un gioco specifico
/// </summary>
public class GameExtractionResult
{
    public required string GameType { get; init; }
    public required Dictionary<string, object?> Fields { get; init; }
    public float Confidence { get; init; }
    public List<string> Warnings { get; init; } = new();
}

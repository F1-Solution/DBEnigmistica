using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per il matching automatico tra giochi e soluzioni
/// </summary>
public interface ISolutionMatchingService
{
    /// <summary>
    /// Trova potenziali soluzioni per i giochi in una pagina
    /// </summary>
    Task<IEnumerable<SolutionMatch>> FindPotentialSolutionsAsync(int pageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Associa una soluzione a un gioco
    /// </summary>
    Task<int> LinkSolutionToGameAsync(int gameId, string solutionText, int? solutionPageId, SolutionType solutionType, double? confidence = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica manualmente una soluzione
    /// </summary>
    Task VerifySolutionAsync(int solutionId, bool isCorrect, string? notes = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Risultato di un match tra gioco e soluzione
/// </summary>
public class SolutionMatch
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public GameType GameType { get; set; }
    public string SolutionText { get; set; } = string.Empty;
    public SolutionType SolutionType { get; set; }
    public double ConfidenceScore { get; set; }
    public int? SolutionPageId { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
}

using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per l'estrazione di soluzioni da testo
/// </summary>
public interface ISolutionExtractionService
{
    /// <summary>
    /// Estrae soluzioni da una pagina
    /// </summary>
    Task<IEnumerable<ExtractedSolution>> ExtractSolutionsFromPageAsync(int pageId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analizza un pattern di soluzione nel testo
    /// </summary>
    Task<IEnumerable<ExtractedSolution>> AnalyzeSolutionPatternAsync(string text, int pageId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Soluzione estratta da una pagina
/// </summary>
public class ExtractedSolution
{
    public string SolutionText { get; set; } = string.Empty;
    public GameType? GameType { get; set; }
    public string? GameReference { get; set; } // es: "Pag. 12", "Cruciverba n. 3"
    public int? ReferencedPageNumber { get; set; }
    public double ConfidenceScore { get; set; }
    public int? PositionX { get; set; }
    public int? PositionY { get; set; }
}

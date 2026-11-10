using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Entità per soluzioni complesse di giochi enigmistici
/// </summary>
public class GameSolution
{
    public int Id { get; set; }
    
    /// <summary>
    /// Riferimento al gioco
    /// </summary>
    public int GameId { get; set; }
    
    /// <summary>
    /// Testo completo della soluzione
    /// </summary>
    public string SolutionText { get; set; } = string.Empty;
    
    /// <summary>
    /// Riferimento opzionale alla pagina contenente la soluzione
    /// </summary>
    public int? SolutionPageId { get; set; }
    
    /// <summary>
    /// Tipo di soluzione in base alla posizione
    /// </summary>
    public SolutionType SolutionType { get; set; }
    
    /// <summary>
    /// Posizione nel layout - coordinata X (top-left)
    /// </summary>
    public int? PositionX { get; set; }
    
    /// <summary>
    /// Posizione nel layout - coordinata Y (top-left)
    /// </summary>
    public int? PositionY { get; set; }
    
    /// <summary>
    /// Confidenza dell'AI nel match (0-1)
    /// </summary>
    public double? ConfidenceScore { get; set; }
    
    /// <summary>
    /// Indica se la soluzione è stata verificata manualmente
    /// </summary>
    public bool IsVerified { get; set; }
    
    /// <summary>
    /// Data di estrazione della soluzione
    /// </summary>
    public DateTime ExtractedDate { get; set; }
    
    /// <summary>
    /// Data di verifica manuale
    /// </summary>
    public DateTime? VerifiedDate { get; set; }
    
    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    public Game Game { get; set; } = null!;
    public Page? SolutionPage { get; set; }
}

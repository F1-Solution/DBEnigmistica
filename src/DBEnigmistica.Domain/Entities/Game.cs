using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Entità base per un gioco enigmistico
/// </summary>
public class Game
{
    public int Id { get; set; }

    /// <summary>
    /// Riferimento alla pagina
    /// </summary>
    public int PageId { get; set; }

    /// <summary>
    /// Tipo di gioco
    /// </summary>
    public GameType GameType { get; set; }

    /// <summary>
    /// Titolo del gioco (se presente)
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Posizione nel layout - coordinata X (top-left)
    /// </summary>
    public int? PositionX { get; set; }

    /// <summary>
    /// Posizione nel layout - coordinata Y (top-left)
    /// </summary>
    public int? PositionY { get; set; }

    /// <summary>
    /// Larghezza del bounding box
    /// </summary>
    public int? BoundingBoxWidth { get; set; }

    /// <summary>
    /// Altezza del bounding box
    /// </summary>
    public int? BoundingBoxHeight { get; set; }

    /// <summary>
    /// Livello di difficoltà (se disponibile)
    /// </summary>
    public string? Difficulty { get; set; }

    /// <summary>
    /// Note o descrizione aggiuntiva
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Data di estrazione
    /// </summary>
    public DateTime ExtractedDate { get; set; }

    /// <summary>
    /// Confidenza dell'AI nella classificazione (0-1)
    /// </summary>
    public double? ClassificationConfidence { get; set; }

    /// <summary>
    /// Soluzione breve del gioco (per soluzioni semplici)
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Riferimento opzionale alla pagina contenente la soluzione
    /// </summary>
    public int? SolutionPageId { get; set; }

    /// <summary>
    /// Indica se il gioco ha una soluzione disponibile
    /// </summary>
    public bool HasSolution { get; set; }

    /// <summary>
    /// Note aggiuntive sulla soluzione
    /// </summary>
    public string? SolutionNotes { get; set; }

    /// <summary>
    /// Origine del Gioco: per un Gioco estratto è sempre Rivista
    /// </summary>
    public required Origine Origine { get; init; }

    // Navigation properties
    public Page Page { get; set; } = null!;
    public Page? SolutionPage { get; set; }
}

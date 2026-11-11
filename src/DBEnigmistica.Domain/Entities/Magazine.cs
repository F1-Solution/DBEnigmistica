using DBEnigmistica.Domain.Enums;

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta una rivista enigmistica
/// </summary>
public class Magazine
{
    public int Id { get; set; }

    /// <summary>
    /// Titolo della rivista (es. "La Settimana Enigmistica")
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Data di pubblicazione
    /// </summary>
    public DateTime PublicationDate { get; set; }

    /// <summary>
    /// Periodicità della pubblicazione
    /// </summary>
    public MagazinePeriodicity Periodicity { get; set; }

    /// <summary>
    /// Numero della rivista (opzionale)
    /// </summary>
    public string? IssueNumber { get; set; }

    /// <summary>
    /// Anno di pubblicazione
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Path del file PDF originale
    /// </summary>
    public required string SourceFilePath { get; set; }

    /// <summary>
    /// Data di elaborazione
    /// </summary>
    public DateTime ProcessedDate { get; set; }

    /// <summary>
    /// Note aggiuntive
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indica se questa rivista pubblica le soluzioni nel numero successivo
    /// </summary>
    public bool SolutionsInNextIssue { get; set; }

    // Navigation properties
    public ICollection<Page> Pages { get; set; } = new List<Page>();
}

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta una pagina di una rivista
/// </summary>
public class Page
{
    public int Id { get; set; }
    
    /// <summary>
    /// Riferimento alla rivista
    /// </summary>
    public int MagazineId { get; set; }
    
    /// <summary>
    /// Numero della pagina nella rivista
    /// </summary>
    public int PageNumber { get; set; }
    
    /// <summary>
    /// Testo estratto tramite OCR (grezzo)
    /// </summary>
    public string? OcrText { get; set; }
    
    /// <summary>
    /// Path dell'immagine della pagina (se salvata)
    /// </summary>
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Larghezza della pagina in pixel
    /// </summary>
    public int? Width { get; set; }
    
    /// <summary>
    /// Altezza della pagina in pixel
    /// </summary>
    public int? Height { get; set; }
    
    /// <summary>
    /// Data di elaborazione OCR
    /// </summary>
    public DateTime? ProcessedDate { get; set; }
    
    // Navigation properties
    public Magazine Magazine { get; set; } = null!;
    public ICollection<Game> Games { get; set; } = new List<Game>();
}

namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta un rebus
/// </summary>
public class RebusGame
{
    public int Id { get; set; }

    /// <summary>
    /// Riferimento al gioco base
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Chiave del rebus (es. "3,5 = 8")
    /// Formato: lunghezze delle parole separate da virgola, totale dopo uguale
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Testo del rebus (lettere che accompagnano le immagini)
    /// </summary>
    public string? RebusText { get; set; }

    /// <summary>
    /// Descrizione delle immagini
    /// </summary>
    public string? ImageDescriptions { get; set; }

    /// <summary>
    /// Path delle immagini estratte (JSON array)
    /// </summary>
    public string? ImagePaths { get; set; }

    /// <summary>
    /// Soluzione del rebus (frase finale)
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Lettura del rebus (come si legge)
    /// </summary>
    public string? Reading { get; set; }

    /// <summary>
    /// Indica se è un rebus figurato
    /// </summary>
    public bool IsPictorial { get; set; }

    // Navigation property
    public Game Game { get; set; } = null!;
}

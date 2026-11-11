namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta un gioco di logica
/// </summary>
public class LogicGame
{
    public int Id { get; set; }

    /// <summary>
    /// Riferimento al gioco base
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Sottotipo di gioco logico (es. "Sudoku", "Kakuro", "Hitori", ecc.)
    /// </summary>
    public string? SubType { get; set; }

    /// <summary>
    /// Testo del problema o delle regole
    /// </summary>
    public string? ProblemText { get; set; }

    /// <summary>
    /// Dati iniziali del gioco (JSON)
    /// Formato dipende dal tipo specifico
    /// </summary>
    public string? InitialData { get; set; }

    /// <summary>
    /// Griglia o struttura del gioco (JSON)
    /// </summary>
    public string? GameStructure { get; set; }

    /// <summary>
    /// Soluzione del gioco (JSON)
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Vincoli o regole specifiche
    /// </summary>
    public string? Constraints { get; set; }

    /// <summary>
    /// Dimensione della griglia (se applicabile)
    /// </summary>
    public string? GridSize { get; set; }

    /// <summary>
    /// Livello di difficoltà stimato (Facile, Medio, Difficile, ecc.)
    /// </summary>
    public string? EstimatedDifficulty { get; set; }

    // Navigation property
    public Game Game { get; set; } = null!;
}

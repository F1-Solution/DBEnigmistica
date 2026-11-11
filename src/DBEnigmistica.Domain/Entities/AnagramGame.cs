namespace DBEnigmistica.Domain.Entities;

/// <summary>
/// Rappresenta un anagramma
/// </summary>
public class AnagramGame
{
    public int Id { get; set; }

    /// <summary>
    /// Riferimento al gioco base
    /// </summary>
    public int GameId { get; set; }

    /// <summary>
    /// Tipo di anagramma (es. "Semplice", "A frase", "Con scarto", ecc.)
    /// </summary>
    public string? AnagramType { get; set; }

    /// <summary>
    /// Parola o frase da anagrammare
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// Indizio o definizione
    /// </summary>
    public string? Clue { get; set; }

    /// <summary>
    /// Soluzione dell'anagramma
    /// </summary>
    public string? Solution { get; set; }

    /// <summary>
    /// Numero di parole nella soluzione
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    /// Lunghezza della soluzione (schema, es. "4,5" per due parole)
    /// </summary>
    public string? LengthPattern { get; set; }

    /// <summary>
    /// Lettere da scartare (se anagramma con scarto)
    /// </summary>
    public string? DiscardedLetters { get; set; }

    /// <summary>
    /// Lettere da aggiungere (se anagramma con cambio)
    /// </summary>
    public string? AddedLetters { get; set; }

    // Navigation property
    public Game Game { get; set; } = null!;
}

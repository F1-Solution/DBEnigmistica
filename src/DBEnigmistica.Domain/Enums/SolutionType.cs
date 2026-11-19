namespace DBEnigmistica.Domain.Enums;

/// <summary>
/// Tipo di soluzione in base alla sua posizione
/// </summary>
public enum SolutionType
{
    /// <summary>
    /// Soluzione nella stessa pagina del gioco
    /// </summary>
    InSamePage = 0,

    /// <summary>
    /// Soluzione in un'altra pagina dello stesso numero
    /// </summary>
    InSameIssue = 1,

    /// <summary>
    /// Soluzione nel numero successivo della rivista
    /// </summary>
    InNextIssue = 2,

    /// <summary>
    /// Soluzione in fonte esterna (es: sito web, altro)
    /// </summary>
    External = 3
}

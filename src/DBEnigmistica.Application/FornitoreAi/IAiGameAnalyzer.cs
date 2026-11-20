using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.Application.FornitoreAi;

/// <summary>
/// Servizio per l'interazione con AI per analisi giochi enigmistici
/// </summary>
public interface IAiGameAnalyzer
{
    /// <summary>
    /// Classifica il tipo di gioco basandosi su testo e metadati
    /// </summary>
    Task<GameClassificationResult> ClassifyGameAsync(
        string text,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera uno schema di tabella per un nuovo tipo di gioco
    /// </summary>
    Task<GameTableSchema> GenerateTableSchemaAsync(
        string gameDescription,
        string? gameTypeName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estrae i campi specifici da un gioco
    /// </summary>
    Task<GameExtractionResult> ExtractGameDataAsync(
        string gameType,
        string text,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analizza una descrizione di gioco da file markdown
    /// </summary>
    Task<GameTableSchema> AnalyzeGameFromMarkdownAsync(
        string markdownContent,
        CancellationToken cancellationToken = default);
}

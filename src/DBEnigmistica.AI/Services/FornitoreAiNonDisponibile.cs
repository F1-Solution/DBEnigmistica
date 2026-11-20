using DBEnigmistica.Application.FornitoreAi;
using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.AI.Services;

/// <summary>Segnaposto del Fornitore AI: ogni chiamata fallisce finché la fase 1 non porta l'implementazione reale.</summary>
public sealed class FornitoreAiNonDisponibile : IAiGameAnalyzer
{
    public const string Messaggio = "Il Fornitore AI è disponibile dalla fase 1";

    public Task<GameClassificationResult> ClassifyGameAsync(
        string text, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Messaggio);

    public Task<GameTableSchema> GenerateTableSchemaAsync(
        string gameDescription, string? gameTypeName = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Messaggio);

    public Task<GameExtractionResult> ExtractGameDataAsync(
        string gameType, string text, Dictionary<string, object>? context = null, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Messaggio);

    public Task<GameTableSchema> AnalyzeGameFromMarkdownAsync(
        string markdownContent, CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(Messaggio);
}

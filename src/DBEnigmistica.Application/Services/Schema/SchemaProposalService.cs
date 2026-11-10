using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Application.Services.Schema;

/// <summary>
/// Servizio per generare proposte di schema usando l'AI
/// </summary>
public class SchemaProposalService : ISchemaProposalService
{
    private readonly ILogger<SchemaProposalService> _logger;
    private readonly IAiGameAnalyzer _aiGameAnalyzer;

    public SchemaProposalService(
        ILogger<SchemaProposalService> logger,
        IAiGameAnalyzer aiGameAnalyzer)
    {
        _logger = logger;
        _aiGameAnalyzer = aiGameAnalyzer;
    }

    public async Task<GameTableSchema> GenerateSchemaProposalAsync(
        string gameTypeName,
        string gameDescription,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating schema proposal for game type: {GameTypeName}", gameTypeName);

        try
        {
            var schema = await _aiGameAnalyzer.GenerateTableSchemaAsync(
                gameDescription,
                gameTypeName,
                cancellationToken);

            _logger.LogInformation("Schema proposal generated: {TableName} with {ColumnCount} columns",
                schema.TableName, schema.Columns.Count);

            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate schema proposal for: {GameTypeName}", gameTypeName);
            throw;
        }
    }

    public async Task<GameTableSchema> GenerateSchemaFromMarkdownAsync(
        string markdownFilePath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating schema from markdown file: {FilePath}", markdownFilePath);

        if (!File.Exists(markdownFilePath))
        {
            throw new FileNotFoundException($"Markdown file not found: {markdownFilePath}");
        }

        var content = await File.ReadAllTextAsync(markdownFilePath, cancellationToken);
        return await GenerateSchemaFromMarkdownContentAsync(content, cancellationToken);
    }

    public async Task<GameTableSchema> GenerateSchemaFromMarkdownContentAsync(
        string markdownContent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating schema from markdown content (length: {Length})", markdownContent.Length);

        try
        {
            var schema = await _aiGameAnalyzer.AnalyzeGameFromMarkdownAsync(
                markdownContent,
                cancellationToken);

            _logger.LogInformation("Schema generated from markdown: {TableName} for game type: {GameTypeName}",
                schema.TableName, schema.GameTypeName);

            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate schema from markdown");
            throw;
        }
    }
}

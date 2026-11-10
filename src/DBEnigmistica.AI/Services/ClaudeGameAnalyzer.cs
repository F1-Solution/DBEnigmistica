using DBEnigmistica.AI.Models;
using DBEnigmistica.AI.Prompts;
using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DBEnigmistica.AI.Services;

/// <summary>
/// Implementazione del servizio di analisi giochi con Claude AI
/// </summary>
public class ClaudeGameAnalyzer : IAiGameAnalyzer
{
    private readonly ILogger<ClaudeGameAnalyzer> _logger;
    private readonly ClaudeApiClient _claudeClient;

    public ClaudeGameAnalyzer(
        ILogger<ClaudeGameAnalyzer> logger,
        ClaudeApiClient claudeClient)
    {
        _logger = logger;
        _claudeClient = claudeClient;
    }

    public async Task<GameClassificationResult> ClassifyGameAsync(
        string text,
        Dictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Classifying game from text (length: {Length})", text.Length);

        var metadataJson = metadata != null 
            ? JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true })
            : "Nessun metadata disponibile";

        var userPrompt = PromptTemplates.GameClassification
            .Replace("{text}", text)
            .Replace("{metadata}", metadataJson);

        var response = await _claudeClient.SendPromptForJsonAsync<ClaudeGameClassificationResponse>(
            "Sei un esperto analista di giochi enigmistici italiani.",
            userPrompt,
            cancellationToken);

        _logger.LogInformation("Game classified as: {GameType} (confidence: {Confidence:P})",
            response.GameType, response.Confidence);

        return new GameClassificationResult
        {
            GameType = response.GameType,
            GameTitle = response.GameTitle,
            Confidence = response.Confidence,
            Description = response.Description,
            Metadata = response.Metadata
        };
    }

    public async Task<GameTableSchema> GenerateTableSchemaAsync(
        string gameDescription,
        string? gameTypeName = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating table schema for game type: {GameTypeName}", gameTypeName ?? "auto");

        var userPrompt = PromptTemplates.SchemaGeneration
            .Replace("{gameDescription}", gameDescription)
            .Replace("{gameTypeName}", gameTypeName ?? "Da determinare dalla descrizione");

        var response = await _claudeClient.SendPromptForJsonAsync<ClaudeSchemaResponse>(
            "Sei un esperto database architect specializzato in PostgreSQL.",
            userPrompt,
            cancellationToken);

        _logger.LogInformation("Generated schema for table: {TableName} with {ColumnCount} columns",
            response.TableName, response.Columns.Count);

        return new GameTableSchema
        {
            GameTypeName = response.GameTypeName,
            TableName = response.TableName,
            Description = response.Description,
            Columns = response.Columns.Select(c => new ColumnSchema
            {
                Name = c.Name,
                DataType = c.DataType,
                IsNullable = c.IsNullable,
                IsRequired = c.IsRequired,
                Description = c.Description,
                DefaultValue = c.DefaultValue,
                MaxLength = c.MaxLength
            }).ToList(),
            Indexes = response.Indexes
        };
    }

    public async Task<GameExtractionResult> ExtractGameDataAsync(
        string gameType,
        string text,
        Dictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting data for game type: {GameType}", gameType);

        var contextJson = context != null
            ? JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true })
            : "Nessun contesto aggiuntivo";

        // Definisci i campi da estrarre in base al tipo di gioco
        var fields = GetFieldsForGameType(gameType);

        var userPrompt = PromptTemplates.DataExtraction
            .Replace("{gameType}", gameType)
            .Replace("{text}", text)
            .Replace("{context}", contextJson)
            .Replace("{fields}", string.Join("\n", fields.Select(f => $"- {f}")));

        var response = await _claudeClient.SendPromptForJsonAsync<ClaudeExtractionResponse>(
            "Sei un esperto di giochi enigmistici italiani.",
            userPrompt,
            cancellationToken);

        _logger.LogInformation("Extracted {FieldCount} fields (confidence: {Confidence:P})",
            response.Fields.Count, response.Confidence);

        // Converti JsonElement in object
        var convertedFields = response.Fields.ToDictionary(
            kvp => kvp.Key,
            kvp => ConvertJsonElement(kvp.Value)
        );

        return new GameExtractionResult
        {
            GameType = response.GameType,
            Fields = convertedFields,
            Confidence = response.Confidence,
            Warnings = response.Warnings
        };
    }

    public async Task<GameTableSchema> AnalyzeGameFromMarkdownAsync(
        string markdownContent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing game from markdown (length: {Length})", markdownContent.Length);

        var userPrompt = PromptTemplates.MarkdownAnalysis
            .Replace("{markdownContent}", markdownContent);

        var response = await _claudeClient.SendPromptForJsonAsync<ClaudeSchemaResponse>(
            "Sei un esperto database architect specializzato in PostgreSQL.",
            userPrompt,
            cancellationToken);

        _logger.LogInformation("Analyzed markdown and generated schema for: {GameTypeName}", response.GameTypeName);

        return new GameTableSchema
        {
            GameTypeName = response.GameTypeName,
            TableName = response.TableName,
            Description = response.Description,
            Columns = response.Columns.Select(c => new ColumnSchema
            {
                Name = c.Name,
                DataType = c.DataType,
                IsNullable = c.IsNullable,
                IsRequired = c.IsRequired,
                Description = c.Description,
                DefaultValue = c.DefaultValue,
                MaxLength = c.MaxLength
            }).ToList(),
            Indexes = response.Indexes
        };
    }

    private List<string> GetFieldsForGameType(string gameType)
    {
        // Definisci i campi standard per ogni tipo di gioco
        return gameType.ToLowerInvariant() switch
        {
            "crossword" => new List<string>
            {
                "Rows: numero di righe",
                "Columns: numero di colonne",
                "HorizontalClues: definizioni orizzontali (JSON)",
                "VerticalClues: definizioni verticali (JSON)",
                "GridStructure: struttura griglia (JSON)"
            },
            "rebus" => new List<string>
            {
                "Key: chiave del rebus",
                "RebusText: testo del rebus",
                "ImageDescriptions: descrizioni immagini",
                "Solution: soluzione"
            },
            "logic" => new List<string>
            {
                "SubType: sottotipo specifico",
                "ProblemText: testo del problema",
                "InitialData: dati iniziali (JSON)",
                "Solution: soluzione (JSON)"
            },
            "anagram" => new List<string>
            {
                "Source: parola/frase originale",
                "Clue: indizio",
                "Solution: soluzione",
                "AnagramType: tipo di anagramma"
            },
            _ => new List<string>
            {
                "Estrai tutti i campi rilevanti identificati nel testo"
            }
        };
    }

    private object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt32(out var i) ? i : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Object => JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText()),
            JsonValueKind.Array => JsonSerializer.Deserialize<List<object>>(element.GetRawText()),
            _ => element.GetRawText()
        };
    }
}

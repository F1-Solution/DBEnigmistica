using System.Text.Json.Serialization;

namespace DBEnigmistica.AI.Models;

/// <summary>
/// Schema di colonna nella risposta di Claude
/// </summary>
public class ClaudeColumnSchema
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("dataType")]
    public required string DataType { get; set; }

    [JsonPropertyName("isNullable")]
    public bool IsNullable { get; set; }

    [JsonPropertyName("isRequired")]
    public bool IsRequired { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("defaultValue")]
    public string? DefaultValue { get; set; }

    [JsonPropertyName("maxLength")]
    public int? MaxLength { get; set; }
}

/// <summary>
/// Risposta dell'AI per la generazione di schema
/// </summary>
public class ClaudeSchemaResponse
{
    [JsonPropertyName("gameTypeName")]
    public required string GameTypeName { get; set; }

    [JsonPropertyName("tableName")]
    public required string TableName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("columns")]
    public List<ClaudeColumnSchema> Columns { get; set; } = new();

    [JsonPropertyName("indexes")]
    public List<string> Indexes { get; set; } = new();
}

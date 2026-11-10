namespace DBEnigmistica.Domain.AI;

/// <summary>
/// Schema di una colonna per una tabella di database
/// </summary>
public class ColumnSchema
{
    public required string Name { get; init; }
    public required string DataType { get; init; }
    public bool IsNullable { get; init; }
    public bool IsRequired { get; init; }
    public string? Description { get; init; }
    public string? DefaultValue { get; init; }
    public int? MaxLength { get; init; }
}

/// <summary>
/// Schema proposto per una nuova tabella di gioco
/// </summary>
public class GameTableSchema
{
    public required string GameTypeName { get; init; }
    public required string TableName { get; init; }
    public required List<ColumnSchema> Columns { get; init; }
    public string? Description { get; init; }
    public List<string> Indexes { get; init; } = new();
}

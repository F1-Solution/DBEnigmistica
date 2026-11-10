using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DBEnigmistica.Application.Services.Schema;

/// <summary>
/// Servizio per validare schemi con l'utente tramite console
/// </summary>
public class SchemaValidationService : ISchemaValidationService
{
    private readonly ILogger<SchemaValidationService> _logger;
    private readonly IGameTypeSchemaRepository _schemaRepository;

    public SchemaValidationService(
        ILogger<SchemaValidationService> logger,
        IGameTypeSchemaRepository schemaRepository)
    {
        _logger = logger;
        _schemaRepository = schemaRepository;
    }

    public async Task<(bool Approved, GameTableSchema? ModifiedSchema)> ValidateSchemaWithUserAsync(
        GameTableSchema proposedSchema,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting user validation for schema: {TableName}", proposedSchema.TableName);

        // Mostra lo schema proposto
        DisplaySchema(proposedSchema);

        Console.WriteLine();
        Console.WriteLine("Vuoi approvare questo schema? (s/n/m per modificare)");
        Console.Write("> ");

        var response = Console.ReadLine()?.Trim().ToLower();

        switch (response)
        {
            case "s":
            case "y":
            case "yes":
            case "si":
            case "sì":
                _logger.LogInformation("Schema approved by user");
                return (true, proposedSchema);

            case "m":
            case "modify":
            case "modifica":
                var modifiedSchema = await ModifySchemaInteractivelyAsync(proposedSchema, cancellationToken);
                return (true, modifiedSchema);

            default:
                _logger.LogInformation("Schema rejected by user");
                return (false, null);
        }
    }

    public async Task<(bool IsValid, List<string> Errors)> ValidateSchemaConsistencyAsync(
        GameTableSchema schema,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating schema consistency for: {TableName}", schema.TableName);

        var errors = new List<string>();

        // Verifica nome tabella
        if (string.IsNullOrWhiteSpace(schema.TableName))
        {
            errors.Add("Table name is required");
        }
        else if (!IsValidTableName(schema.TableName))
        {
            errors.Add($"Invalid table name: {schema.TableName}. Use snake_case format");
        }

        // Verifica se la tabella esiste già
        if (await _schemaRepository.TableNameExistsAsync(schema.TableName))
        {
            errors.Add($"Table name '{schema.TableName}' already exists");
        }

        // Verifica colonne
        if (!schema.Columns.Any())
        {
            errors.Add("At least one column is required");
        }

        foreach (var column in schema.Columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
            {
                errors.Add("Column name cannot be empty");
            }
            else if (!IsValidColumnName(column.Name))
            {
                errors.Add($"Invalid column name: {column.Name}");
            }

            if (string.IsNullOrWhiteSpace(column.DataType))
            {
                errors.Add($"Data type is required for column: {column.Name}");
            }
        }

        // Verifica colonne duplicate
        var duplicateColumns = schema.Columns
            .GroupBy(c => c.Name.ToLower())
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var duplicate in duplicateColumns)
        {
            errors.Add($"Duplicate column name: {duplicate}");
        }

        var isValid = !errors.Any();
        
        if (!isValid)
        {
            _logger.LogWarning("Schema validation failed with {ErrorCount} errors", errors.Count);
        }

        return (isValid, errors);
    }

    private void DisplaySchema(GameTableSchema schema)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"PROPOSTA SCHEMA: {schema.GameTypeName}");
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"Nome Tabella: {schema.TableName}");
        
        if (!string.IsNullOrEmpty(schema.Description))
        {
            Console.WriteLine($"Descrizione: {schema.Description}");
        }

        Console.WriteLine();
        Console.WriteLine("Colonne:");
        Console.WriteLine("─────────────────────────────────────────────────────");

        foreach (var column in schema.Columns)
        {
            var nullable = column.IsNullable ? "NULL" : "NOT NULL";
            var required = column.IsRequired ? " [REQUIRED]" : "";
            
            Console.WriteLine($"  • {column.Name,-30} {column.DataType,-15} {nullable}{required}");
            
            if (!string.IsNullOrEmpty(column.Description))
            {
                Console.WriteLine($"    └─ {column.Description}");
            }
        }

        if (schema.Indexes.Any())
        {
            Console.WriteLine();
            Console.WriteLine("Indexes: " + string.Join(", ", schema.Indexes));
        }

        Console.WriteLine("═══════════════════════════════════════════════════════");
    }

    private async Task<GameTableSchema> ModifySchemaInteractivelyAsync(
        GameTableSchema originalSchema,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting interactive schema modification");

        Console.WriteLine();
        Console.WriteLine("MODALITÀ MODIFICA SCHEMA");
        Console.WriteLine("Comandi disponibili:");
        Console.WriteLine("  - rename-table <nuovo_nome>");
        Console.WriteLine("  - add-column <nome> <tipo> [nullable]");
        Console.WriteLine("  - remove-column <nome>");
        Console.WriteLine("  - modify-column <nome> <nuovo_tipo>");
        Console.WriteLine("  - done (per finire)");
        Console.WriteLine();

        var modifiedColumns = new List<ColumnSchema>(originalSchema.Columns);
        var modifiedTableName = originalSchema.TableName;

        while (true)
        {
            Console.Write("Modifica> ");
            var command = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(command))
                continue;

            var parts = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length == 0)
                continue;

            switch (parts[0].ToLower())
            {
                case "done":
                case "fine":
                case "exit":
                    return new GameTableSchema
                    {
                        GameTypeName = originalSchema.GameTypeName,
                        TableName = modifiedTableName,
                        Description = originalSchema.Description,
                        Columns = modifiedColumns,
                        Indexes = originalSchema.Indexes
                    };

                case "rename-table":
                    if (parts.Length >= 2)
                    {
                        modifiedTableName = parts[1];
                        Console.WriteLine($"✓ Tabella rinominata in: {modifiedTableName}");
                    }
                    break;

                case "add-column":
                    if (parts.Length >= 3)
                    {
                        var newColumn = new ColumnSchema
                        {
                            Name = parts[1],
                            DataType = parts[2],
                            IsNullable = parts.Length > 3 && parts[3].ToLower() == "nullable",
                            IsRequired = false,
                            Description = null
                        };
                        modifiedColumns.Add(newColumn);
                        Console.WriteLine($"✓ Colonna aggiunta: {newColumn.Name} {newColumn.DataType}");
                    }
                    break;

                case "remove-column":
                    if (parts.Length >= 2)
                    {
                        var removed = modifiedColumns.RemoveAll(c => c.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
                        Console.WriteLine(removed > 0 ? $"✓ Colonna rimossa: {parts[1]}" : $"✗ Colonna non trovata: {parts[1]}");
                    }
                    break;

                default:
                    Console.WriteLine("Comando non riconosciuto");
                    break;
            }
        }
    }

    private bool IsValidTableName(string tableName)
    {
        // Verifica che il nome sia in snake_case e contenga solo caratteri validi
        return !string.IsNullOrWhiteSpace(tableName) &&
               tableName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
               !char.IsDigit(tableName[0]);
    }

    private bool IsValidColumnName(string columnName)
    {
        return !string.IsNullOrWhiteSpace(columnName) &&
               columnName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
               !char.IsDigit(columnName[0]);
    }
}

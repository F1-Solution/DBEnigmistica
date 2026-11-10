using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Application.Services.Schema;

/// <summary>
/// Servizio per validare schemi, con la revisione dell'utente delegata a <see cref="ISchemaReviewInteraction"/>.
/// </summary>
public class SchemaValidationService : ISchemaValidationService
{
    // Colonne gestite dal sistema in ogni tabella di tipologia.
    private static readonly HashSet<string> ReservedColumnNames =
        new(["id", "game_id"], StringComparer.OrdinalIgnoreCase);

    private static readonly string[] ApprovalAnswers = ["s", "y", "yes", "si", "sì"];
    private static readonly string[] ModifyAnswers = ["m", "modify", "modifica"];
    private static readonly string[] EndEditAnswers = ["done", "fine", "exit"];

    private readonly ILogger<SchemaValidationService> _logger;
    private readonly IGameTypeSchemaRepository _schemaRepository;
    private readonly ISchemaReviewInteraction? _interaction;

    public SchemaValidationService(
        ILogger<SchemaValidationService> logger,
        IGameTypeSchemaRepository schemaRepository,
        ISchemaReviewInteraction? interaction = null)
    {
        _logger = logger;
        _schemaRepository = schemaRepository;
        _interaction = interaction;
    }

    public async Task<(bool Approved, GameTableSchema? ModifiedSchema)> ValidateSchemaWithUserAsync(
        GameTableSchema proposedSchema,
        CancellationToken cancellationToken = default)
    {
        var interaction = _interaction
            ?? throw new InvalidOperationException("Nessuna interazione con l'utente disponibile per la revisione dello schema.");

        _logger.LogInformation("Starting user validation for schema: {TableName}", proposedSchema.TableName);

        interaction.ShowProposal(proposedSchema);
        var response = interaction.AskApproval()?.Trim().ToLowerInvariant();

        if (response is not null && ApprovalAnswers.Contains(response))
        {
            _logger.LogInformation("Schema approved by user");
            return (true, proposedSchema);
        }

        if (response is not null && ModifyAnswers.Contains(response))
        {
            var modified = await ModifySchemaInteractivelyAsync(interaction, proposedSchema, cancellationToken);
            return (true, modified);
        }

        _logger.LogInformation("Schema rejected by user");
        return (false, null);
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
            errors.Add("Nome tabella obbligatorio");
        }
        else if (!IsValidTableName(schema.TableName))
        {
            errors.Add($"Nome tabella non valido: {schema.TableName}. Usare il formato snake_case");
        }

        // Verifica se la tabella esiste già
        if (await _schemaRepository.TableNameExistsAsync(schema.TableName))
        {
            errors.Add($"Il nome tabella '{schema.TableName}' esiste già");
        }

        // Verifica colonne
        if (!schema.Columns.Any())
        {
            errors.Add("Serve almeno una colonna");
        }

        foreach (var column in schema.Columns)
        {
            if (string.IsNullOrWhiteSpace(column.Name))
            {
                errors.Add("Il nome della colonna non può essere vuoto");
            }
            else if (!IsValidColumnName(column.Name))
            {
                errors.Add($"Colonna '{column.Name}': nome colonna non valido");
            }
            else if (ReservedColumnNames.Contains(column.Name))
            {
                errors.Add($"Colonna '{column.Name}': nome riservato");
            }

            if (string.IsNullOrWhiteSpace(column.DataType))
            {
                errors.Add($"Tipo di dato obbligatorio per la colonna: {column.Name}");
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
            errors.Add($"Colonne duplicate: {duplicate}");
        }

        var isValid = !errors.Any();

        if (!isValid)
        {
            _logger.LogWarning("Schema validation failed with {ErrorCount} errors", errors.Count);
        }

        return (isValid, errors);
    }

    private Task<GameTableSchema> ModifySchemaInteractivelyAsync(
        ISchemaReviewInteraction interaction,
        GameTableSchema original,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting interactive schema modification");
        interaction.ShowEditHelp();

        var columns = new List<ColumnSchema>(original.Columns);
        var tableName = original.TableName;

        while (!cancellationToken.IsCancellationRequested)
        {
            var line = interaction.AskEditCommand()?.Trim();

            // Fine dell'input: si chiude la modifica con quanto fatto finora.
            if (line is null)
                break;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            if (EndEditAnswers.Contains(parts[0], StringComparer.OrdinalIgnoreCase))
                break;

            var (newName, message) = ApplyEdit(parts, columns, tableName);
            tableName = newName;
            interaction.ShowEditResult(message);
        }

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(new GameTableSchema
        {
            GameTypeName = original.GameTypeName,
            TableName = tableName,
            Description = original.Description,
            Columns = columns,
            Indexes = original.Indexes
        });
    }

    private static (string TableName, string Message) ApplyEdit(
        string[] parts,
        List<ColumnSchema> columns,
        string tableName)
    {
        switch (parts[0].ToLowerInvariant())
        {
            case "rename-table" when parts.Length >= 2:
                return (parts[1], $"✓ Tabella rinominata in: {parts[1]}");

            case "add-column" when parts.Length >= 3:
                columns.Add(new ColumnSchema
                {
                    Name = parts[1],
                    DataType = parts[2],
                    IsNullable = parts.Length > 3 && parts[3].Equals("nullable", StringComparison.OrdinalIgnoreCase)
                });
                return (tableName, $"✓ Colonna aggiunta: {parts[1]} {parts[2]}");

            case "remove-column" when parts.Length >= 2:
                var removed = columns.RemoveAll(c => c.Name.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
                return (tableName, removed > 0 ? $"✓ Colonna rimossa: {parts[1]}" : $"✗ Colonna non trovata: {parts[1]}");

            case "modify-column" when parts.Length >= 3:
                return (tableName, ChangeColumnType(columns, parts[1], parts[2]));

            default:
                return (tableName, "Comando non riconosciuto");
        }
    }

    private static string ChangeColumnType(List<ColumnSchema> columns, string name, string newType)
    {
        var index = columns.FindIndex(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return $"✗ Colonna non trovata: {name}";

        var old = columns[index];
        columns[index] = new ColumnSchema
        {
            Name = old.Name,
            DataType = newType,
            IsNullable = old.IsNullable,
            IsRequired = old.IsRequired,
            Description = old.Description,
            DefaultValue = old.DefaultValue,
            MaxLength = old.MaxLength
        };
        return $"✓ Colonna modificata: {name} {newType}";
    }

    private static bool IsValidTableName(string tableName)
    {
        // Verifica che il nome sia in snake_case e contenga solo caratteri validi
        return !string.IsNullOrWhiteSpace(tableName) &&
               tableName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
               !char.IsDigit(tableName[0]);
    }

    private static bool IsValidColumnName(string columnName)
    {
        return !string.IsNullOrWhiteSpace(columnName) &&
               columnName.All(c => char.IsLetterOrDigit(c) || c == '_') &&
               !char.IsDigit(columnName[0]);
    }
}

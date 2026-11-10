using DBEnigmistica.Domain.AI;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;

namespace DBEnigmistica.Infrastructure.Services.Database;

/// <summary>
/// Servizio per applicare migrazioni dinamiche al database PostgreSQL
/// </summary>
public class DynamicMigrationService : IDynamicMigrationService
{
    private readonly ILogger<DynamicMigrationService> _logger;
    private readonly EnigmisticaDbContext _context;

    public DynamicMigrationService(
        ILogger<DynamicMigrationService> logger,
        EnigmisticaDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<bool> CreateTableAsync(
        GameTableSchema schema,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating table: {TableName}", schema.TableName);

        try
        {
            // Genera lo script SQL
            var sql = GenerateCreateTableSql(schema);

            _logger.LogDebug("Generated SQL: {Sql}", sql);

            // Esegui lo script
            await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);

            _logger.LogInformation("Table created successfully: {TableName}", schema.TableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create table: {TableName}", schema.TableName);
            return false;
        }
    }

    public async Task<bool> TableExistsAsync(
        string tableName,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT EXISTS (
                SELECT FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_name = {0}
            )";

        try
        {
            var exists = await _context.Database
                .SqlQueryRaw<bool>(sql, tableName.ToLower())
                .FirstOrDefaultAsync(cancellationToken);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if table exists: {TableName}", tableName);
            return false;
        }
    }

    public string GenerateCreateTableSql(GameTableSchema schema)
    {
        var sql = new StringBuilder();

        sql.AppendLine($"CREATE TABLE IF NOT EXISTS {schema.TableName} (");
        sql.AppendLine("    id SERIAL PRIMARY KEY,");
        sql.AppendLine("    game_id INTEGER NOT NULL,");

        // Aggiungi le colonne definite
        foreach (var column in schema.Columns)
        {
            var columnDef = BuildColumnDefinition(column);
            sql.AppendLine($"    {columnDef},");
        }

        // Aggiungi la foreign key
        sql.AppendLine("    CONSTRAINT fk_game FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE");
        sql.AppendLine(");");

        // Aggiungi gli indici
        if (schema.Indexes.Any())
        {
            sql.AppendLine();
            foreach (var index in schema.Indexes)
            {
                var indexName = $"idx_{schema.TableName}_{index.ToLower()}";
                sql.AppendLine($"CREATE INDEX IF NOT EXISTS {indexName} ON {schema.TableName}({index.ToLower()});");
            }
        }

        // Indice automatico su game_id
        sql.AppendLine($"CREATE INDEX IF NOT EXISTS idx_{schema.TableName}_game_id ON {schema.TableName}(game_id);");

        return sql.ToString();
    }

    public async Task<bool> ExecuteSqlAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Executing custom SQL (use with caution)");

        try
        {
            await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
            _logger.LogInformation("SQL executed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute SQL");
            return false;
        }
    }

    private string BuildColumnDefinition(ColumnSchema column)
    {
        var def = new StringBuilder();
        
        def.Append(column.Name.ToLower());
        def.Append(' ');
        def.Append(MapDataType(column.DataType));

        // MaxLength per varchar
        if (column.MaxLength.HasValue && 
            (column.DataType.ToLower().Contains("varchar") || column.DataType.ToLower().Contains("char")))
        {
            def.Append($"({column.MaxLength.Value})");
        }

        // Nullable
        if (!column.IsNullable || column.IsRequired)
        {
            def.Append(" NOT NULL");
        }

        // Default value
        if (!string.IsNullOrEmpty(column.DefaultValue))
        {
            def.Append($" DEFAULT {column.DefaultValue}");
        }

        return def.ToString();
    }

    private string MapDataType(string dataType)
    {
        // Mappa i tipi di dati al formato PostgreSQL
        var type = dataType.ToLower().Trim();

        return type switch
        {
            "string" => "TEXT",
            "int" => "INTEGER",
            "integer" => "INTEGER",
            "long" => "BIGINT",
            "bigint" => "BIGINT",
            "float" => "REAL",
            "double" => "DOUBLE PRECISION",
            "decimal" => "NUMERIC",
            "bool" => "BOOLEAN",
            "boolean" => "BOOLEAN",
            "datetime" => "TIMESTAMP",
            "date" => "DATE",
            "time" => "TIME",
            "json" => "JSONB",
            "jsonb" => "JSONB",
            "text" => "TEXT",
            _ => type.ToUpper() // Usa il tipo così com'è se già in formato PostgreSQL
        };
    }
}

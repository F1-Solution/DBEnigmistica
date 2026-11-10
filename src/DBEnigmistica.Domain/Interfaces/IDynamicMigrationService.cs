using DBEnigmistica.Domain.AI;

namespace DBEnigmistica.Domain.Interfaces;

/// <summary>
/// Servizio per applicare migrazioni dinamiche al database
/// </summary>
public interface IDynamicMigrationService
{
    /// <summary>
    /// Crea una nuova tabella nel database in base allo schema fornito
    /// </summary>
    Task<bool> CreateTableAsync(
        GameTableSchema schema,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se una tabella esiste già nel database
    /// </summary>
    Task<bool> TableExistsAsync(
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ottiene lo script SQL che verrebbe eseguito per creare la tabella
    /// </summary>
    string GenerateCreateTableSql(GameTableSchema schema);

    /// <summary>
    /// Esegue uno script SQL personalizzato (da usare con cautela)
    /// </summary>
    Task<bool> ExecuteSqlAsync(
        string sql,
        CancellationToken cancellationToken = default);
}

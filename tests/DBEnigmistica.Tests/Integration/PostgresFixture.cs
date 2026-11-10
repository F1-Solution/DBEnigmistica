using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>
/// Un solo contenitore PostgreSQL per tutta la raccolta di test.
/// Le migrazioni EF si applicano una volta sola a un database modello; ogni test riceve
/// una copia del modello (CREATE DATABASE ... TEMPLATE), quindi i test sono isolati
/// e nessuno vede i dati degli altri.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string TemplateDatabase = "enigmistica_template";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await ExecuteAdminAsync($"CREATE DATABASE \"{TemplateDatabase}\"");

        var options = new DbContextOptionsBuilder<EnigmisticaDbContext>()
            .UseNpgsql(ConnectionStringFor(TemplateDatabase))
            .Options;

        await using (var context = new EnigmisticaDbContext(options))
        {
            await context.Database.MigrateAsync();
        }

        // CREATE DATABASE ... TEMPLATE richiede che nessuna sessione resti aperta sul modello.
        NpgsqlConnection.ClearAllPools();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task<string> CreateDatabaseAsync()
    {
        var name = $"test_{Guid.NewGuid():N}";
        await ExecuteAdminAsync($"CREATE DATABASE \"{name}\" TEMPLATE \"{TemplateDatabase}\"");
        return name;
    }

    public async Task DropDatabaseAsync(string name)
    {
        // Se la creazione è fallita il nome è vuoto e non c'è nulla da eliminare.
        if (string.IsNullOrWhiteSpace(name))
            return;

        // Svuota il pool di quel database così il DROP non trova connessioni pendenti.
        using (var connection = new NpgsqlConnection(ConnectionStringFor(name)))
            NpgsqlConnection.ClearPool(connection);

        await ExecuteAdminAsync($"DROP DATABASE IF EXISTS \"{name}\" WITH (FORCE)");
    }

    public string ConnectionStringFor(string database)
    {
        var builder = new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = database
        };
        return builder.ConnectionString;
    }

    private async Task ExecuteAdminAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionStringFor("postgres"));
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class PostgresCollection : ICollectionFixture<PostgresFixture>
{
    public const string Name = "PostgreSQL";
}

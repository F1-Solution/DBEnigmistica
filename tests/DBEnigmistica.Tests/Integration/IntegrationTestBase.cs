using DBEnigmistica.Domain.Catalogo;
using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>
/// Ogni istanza (cioe' ogni test) lavora su un database proprio, copia del modello migrato.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private string _databaseName = string.Empty;

    protected IntegrationTestBase(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    protected string ConnectionString => _fixture.ConnectionStringFor(_databaseName);

    public async Task InitializeAsync()
    {
        _databaseName = await _fixture.CreateDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DropDatabaseAsync(_databaseName);
    }

    /// <summary>
    /// Nuovo contesto sul database del test: usarne uno diverso per scrivere e per verificare
    /// evita di leggere dalla cache del tracciamento invece che dal database.
    /// </summary>
    protected EnigmisticaDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<EnigmisticaDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new EnigmisticaDbContext(options);
    }

    protected async Task<T> ScalarAsync<T>(string sql)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    protected static NpgsqlParameter P(string name, object? value) => new(name, value ?? DBNull.Value);

    protected async Task<int> ExecuteAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>Esegue l'SQL (se indicato, con SET ROLE) e restituisce lo SQLSTATE dell'errore, o null se riesce.</summary>
    protected async Task<string?> SqlStateAsync(string sql, string? role = null, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        if (role is not null)
        {
            await using var set = new NpgsqlCommand($"SET ROLE {role}", connection);
            await set.ExecuteNonQueryAsync();
        }

        try
        {
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddRange(parameters);
            await command.ExecuteNonQueryAsync();
            return null;
        }
        catch (PostgresException e)
        {
            return e.SqlState;
        }
    }

    protected async Task<Soggetto> SeedSoggettoAsync(string denominazione = "Soggetto di prova")
    {
        await using var context = NewContext();
        var soggetto = Soggetto.Crea(TipoSoggetto.Organizzazione, denominazione, null, null, DateTime.UtcNow);
        context.Soggetti.Add(soggetto);
        await context.SaveChangesAsync();
        return soggetto;
    }

    protected async Task<Guid> SeedGiocoOriginaleAsync(
        Origine origine = Origine.Originale, Action<GiocoOriginale>? configure = null)
    {
        await using var context = NewContext();
        var gioco = GiocoOriginale.Crea("cruciverba-classico", 1, origine, DateTime.UtcNow, "Titolo");
        configure?.Invoke(gioco);
        context.GiochiOriginali.Add(gioco);
        await context.SaveChangesAsync();
        return gioco.Id;
    }

    protected static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    protected static Magazine NewMagazine(
        string title = "La Settimana Enigmistica",
        DateTime? publicationDate = null,
        int year = 2024,
        MagazinePeriodicity periodicity = MagazinePeriodicity.Weekly) => new()
        {
            Title = title,
            PublicationDate = publicationDate ?? Utc(year, 1, 15),
            Periodicity = periodicity,
            Year = year,
            SourceFilePath = $"/riviste/{Guid.NewGuid():N}.pdf",
            ProcessedDate = DateTime.UtcNow
        };

    protected static Page NewPage(int magazineId, int pageNumber = 1) => new()
    {
        MagazineId = magazineId,
        PageNumber = pageNumber,
        ProcessedDate = DateTime.UtcNow
    };

    protected static Game NewGame(
        int pageId,
        GameType gameType = GameType.Crossword,
        string? title = "Gioco",
        int? x = null,
        int? y = null) => new()
        {
            Origine = Origine.Rivista,
            PageId = pageId,
            GameType = gameType,
            Title = title,
            PositionX = x,
            PositionY = y,
            ExtractedDate = DateTime.UtcNow
        };

    protected static GameSolution NewSolution(
        int gameId,
        string text = "SOLUZIONE",
        bool verified = false,
        int? solutionPageId = null,
        SolutionType type = SolutionType.InSameIssue,
        DateTime? extractedDate = null) => new()
        {
            GameId = gameId,
            SolutionText = text,
            IsVerified = verified,
            SolutionPageId = solutionPageId,
            SolutionType = type,
            ExtractedDate = extractedDate ?? DateTime.UtcNow
        };

    /// <summary>
    /// Salva una rivista con una pagina per ciascun numero indicato e restituisce gli identificativi.
    /// </summary>
    protected async Task<(int MagazineId, int[] PageIds)> SeedMagazineWithPagesAsync(
        int pageCount = 1,
        Magazine? magazine = null)
    {
        await using var context = NewContext();
        var entity = magazine ?? NewMagazine();
        context.Magazines.Add(entity);
        await context.SaveChangesAsync();

        var pages = Enumerable.Range(1, pageCount).Select(n => NewPage(entity.Id, n)).ToList();
        context.Pages.AddRange(pages);
        await context.SaveChangesAsync();

        return (entity.Id, pages.Select(p => p.Id).ToArray());
    }

    protected async Task<int> SeedGameAsync(int pageId, Action<Game>? configure = null)
    {
        await using var context = NewContext();
        var game = NewGame(pageId);
        configure?.Invoke(game);
        context.Games.Add(game);
        await context.SaveChangesAsync();
        return game.Id;
    }
}

using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DBEnigmistica.Tests.Integration;

/// <summary>
/// Vincoli che solo un PostgreSQL vero fa rispettare: chiavi esterne, cascata, unicita', lunghezze.
/// </summary>
[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class DatabaseConstraintsIntegrationTests : IntegrationTestBase
{
    public DatabaseConstraintsIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    private static void ShouldViolate(Exception? exception, string sqlState)
    {
        var postgres = exception.Should().BeOfType<DbUpdateException>().Subject
            .InnerException.Should().BeOfType<PostgresException>().Subject;
        postgres.SqlState.Should().Be(sqlState);
    }

    [Fact]
    public async Task Page_of_a_missing_magazine_is_rejected_by_the_foreign_key()
    {
        await using var context = NewContext();
        context.Pages.Add(NewPage(magazineId: 9999));

        var save = async () => await context.SaveChangesAsync();

        var exception = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
        ShouldViolate(exception, PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Game_of_a_missing_page_is_rejected_by_the_foreign_key()
    {
        await using var context = NewContext();
        context.Games.Add(NewGame(pageId: 9999));

        var save = async () => await context.SaveChangesAsync();

        var exception = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
        ShouldViolate(exception, PostgresErrorCodes.ForeignKeyViolation);
    }

    [Fact]
    public async Task Deleting_a_magazine_removes_its_pages_and_games()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[0]);
        await SeedGameAsync(pageIds[1]);

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            await repository.DeleteAsync(magazineId);
            await repository.SaveChangesAsync();
        }

        (await ScalarAsync<long>("SELECT count(*) FROM riviste.magazines")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.pages")).Should().Be(0);
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(0);
    }

    [Fact]
    public async Task Deleting_a_page_removes_only_its_own_games()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[0]);
        await SeedGameAsync(pageIds[1]);

        await using (var write = NewContext())
        {
            write.Pages.Remove((await write.Pages.FindAsync(pageIds[0]))!);
            await write.SaveChangesAsync();
        }

        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(1);
        (await ScalarAsync<int>("SELECT \"PageId\" FROM riviste.games")).Should().Be(pageIds[1]);
    }

    [Fact]
    public async Task Magazine_title_longer_than_200_characters_is_rejected()
    {
        await using var context = NewContext();
        context.Magazines.Add(NewMagazine(title: new string('T', 201)));

        var save = async () => await context.SaveChangesAsync();

        var exception = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
        ShouldViolate(exception, PostgresErrorCodes.StringDataRightTruncation);
    }

    [Fact]
    public async Task Two_schemas_cannot_share_the_same_table_name()
    {
        await using (var first = NewContext())
        {
            first.GameTypeSchemas.Add(NewSchema("Sudoku", "sudoku_games"));
            await first.SaveChangesAsync();
        }

        await using var second = NewContext();
        second.GameTypeSchemas.Add(NewSchema("Sudoku bis", "sudoku_games"));

        var save = async () => await second.SaveChangesAsync();

        var exception = (await save.Should().ThrowAsync<DbUpdateException>()).Which;
        ShouldViolate(exception, PostgresErrorCodes.UniqueViolation);
    }

    [Fact]
    public async Task Schema_lookup_by_name_and_by_table_ignores_case()
    {
        await using (var write = NewContext())
        {
            write.GameTypeSchemas.Add(NewSchema("Sudoku", "sudoku_games", validated: true));
            write.GameTypeSchemas.Add(NewSchema("Quiz", "quiz_games", validated: false));
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var repository = new GameTypeSchemaRepository(read);

        (await repository.GetByGameTypeNameAsync("SUDOKU"))!.TableName.Should().Be("sudoku_games");
        (await repository.GetByTableNameAsync("QUIZ_GAMES"))!.GameTypeName.Should().Be("Quiz");
        (await repository.TableNameExistsAsync("Sudoku_Games")).Should().BeTrue();
        (await repository.TableNameExistsAsync("missing")).Should().BeFalse();
        (await repository.GetAllValidatedAsync()).Select(s => s.GameTypeName).Should().Equal("Sudoku");
    }

    private static GameTypeSchema NewSchema(string name, string table, bool validated = false) => new()
    {
        GameTypeName = name,
        TableName = table,
        SchemaJson = "[]",
        CreatedDate = DateTime.UtcNow,
        Version = 1,
        IsUserValidated = validated
    };
}

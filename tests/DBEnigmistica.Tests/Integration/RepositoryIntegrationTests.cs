using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;

namespace DBEnigmistica.Tests.Integration;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class RepositoryIntegrationTests : IntegrationTestBase
{
    public RepositoryIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Saved_magazine_is_read_back_with_all_its_fields()
    {
        var magazine = NewMagazine(periodicity: MagazinePeriodicity.Monthly);
        magazine.IssueNumber = "4500";
        magazine.SolutionsInNextIssue = true;
        magazine.Notes = "numero di prova";

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            await repository.AddAsync(magazine);
            await repository.SaveChangesAsync();
        }

        await using var read = NewContext();
        var loaded = await new MagazineRepository(read).GetByIdAsync(magazine.Id);

        magazine.Id.Should().BeGreaterThan(0);
        loaded.Should().NotBeNull();
        loaded!.Title.Should().Be("La Settimana Enigmistica");
        loaded.IssueNumber.Should().Be("4500");
        loaded.Periodicity.Should().Be(MagazinePeriodicity.Monthly);
        loaded.SolutionsInNextIssue.Should().BeTrue();
        loaded.Notes.Should().Be("numero di prova");
        loaded.PublicationDate.Should().Be(Utc(2024, 1, 15));
    }

    [Fact]
    public async Task Periodicity_and_game_type_are_stored_as_text_in_the_database()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(magazine: NewMagazine(periodicity: MagazinePeriodicity.Weekly));
        await SeedGameAsync(pageIds[0], g => g.GameType = GameType.Rebus);

        (await ScalarAsync<string>("SELECT \"Periodicity\" FROM magazines")).Should().Be("Weekly");
        (await ScalarAsync<string>("SELECT \"GameType\" FROM games")).Should().Be("Rebus");
    }

    [Fact]
    public async Task Updated_magazine_keeps_the_change()
    {
        var (magazineId, _) = await SeedMagazineWithPagesAsync(0);

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            var magazine = (await repository.GetByIdAsync(magazineId))!;
            magazine.IssueNumber = "4501";
            await repository.UpdateAsync(magazine);
            await repository.SaveChangesAsync();
        }

        await using var read = NewContext();
        (await new MagazineRepository(read).GetByIdAsync(magazineId))!.IssueNumber.Should().Be("4501");
    }

    [Fact]
    public async Task Deleted_magazine_is_no_longer_found()
    {
        var (magazineId, _) = await SeedMagazineWithPagesAsync(0);

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            await repository.DeleteAsync(magazineId);
            await repository.SaveChangesAsync();
        }

        await using var read = NewContext();
        (await new MagazineRepository(read).GetByIdAsync(magazineId)).Should().BeNull();
    }

    [Fact]
    public async Task Magazine_is_found_by_title_and_day_ignoring_the_time_of_day()
    {
        await SeedMagazineWithPagesAsync(0, NewMagazine("La Settimana Enigmistica", Utc(2024, 1, 15)));
        await SeedMagazineWithPagesAsync(0, NewMagazine("Enigmistica Italiana", Utc(2024, 1, 15)));
        await SeedMagazineWithPagesAsync(0, NewMagazine("La Settimana Enigmistica", Utc(2024, 1, 22)));

        await using var read = NewContext();
        var found = await new MagazineRepository(read)
            .GetByTitleAndDateAsync("La Settimana Enigmistica", new DateTime(2024, 1, 15, 14, 30, 0, DateTimeKind.Utc));

        found.Should().NotBeNull();
        found!.Title.Should().Be("La Settimana Enigmistica");
        found.PublicationDate.Date.Should().Be(new DateTime(2024, 1, 15));
    }

    [Fact]
    public async Task Magazine_lookup_by_title_and_date_returns_nothing_when_no_issue_matches()
    {
        await SeedMagazineWithPagesAsync(0, NewMagazine("La Settimana Enigmistica", Utc(2024, 1, 15)));

        await using var read = NewContext();
        var found = await new MagazineRepository(read)
            .GetByTitleAndDateAsync("La Settimana Enigmistica", Utc(2024, 1, 16));

        found.Should().BeNull();
    }

    [Fact]
    public async Task Magazines_of_a_year_are_returned_oldest_first()
    {
        await SeedMagazineWithPagesAsync(0, NewMagazine("B", Utc(2024, 3, 1), 2024));
        await SeedMagazineWithPagesAsync(0, NewMagazine("A", Utc(2024, 1, 1), 2024));
        await SeedMagazineWithPagesAsync(0, NewMagazine("C", Utc(2023, 6, 1), 2023));

        await using var read = NewContext();
        var magazines = (await new MagazineRepository(read).GetByYearAsync(2024)).ToList();

        magazines.Select(m => m.Title).Should().Equal("A", "B");
    }

    [Fact]
    public async Task Magazine_with_pages_loads_all_of_its_pages()
    {
        var (magazineId, _) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedMagazineWithPagesAsync(pageCount: 3);

        await using var read = NewContext();
        var loaded = await new MagazineRepository(read).GetWithPagesAsync(magazineId);

        loaded.Should().NotBeNull();
        loaded!.Pages.Select(p => p.PageNumber).Should().BeEquivalentTo(new[] { 1, 2 });
    }

    [Fact]
    public async Task Page_added_through_the_repository_belongs_to_the_magazine()
    {
        var (magazineId, _) = await SeedMagazineWithPagesAsync(0);

        await using (var write = NewContext())
        {
            var repository = new MagazineRepository(write);
            await repository.AddPageAsync(new Page { MagazineId = magazineId, PageNumber = 7, Width = 800, Height = 1200 });
            await repository.SaveChangesAsync();
        }

        await using var read = NewContext();
        var loaded = await new MagazineRepository(read).GetWithPagesAsync(magazineId);
        loaded!.Pages.Should().ContainSingle(p => p.PageNumber == 7 && p.Width == 800 && p.Height == 1200);
    }

    [Fact]
    public async Task Magazine_with_pages_and_games_loads_the_whole_tree()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[0], g => g.Title = "Primo");
        await SeedGameAsync(pageIds[0], g => g.Title = "Secondo");
        await SeedGameAsync(pageIds[1], g => g.Title = "Terzo");

        await using var read = NewContext();
        var loaded = await new MagazineRepository(read).GetWithPagesAndGamesAsync(magazineId);

        loaded!.Pages.Should().HaveCount(2);
        loaded.Pages.SelectMany(p => p.Games).Select(g => g.Title)
            .Should().BeEquivalentTo(new[] { "Primo", "Secondo", "Terzo" });
    }

    [Fact]
    public async Task Saved_game_is_read_back_with_all_its_fields()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        var game = new Game
        {
            PageId = pageIds[0],
            GameType = GameType.Crossword,
            Title = "Cruciverba Facile",
            Description = "Un cruciverba semplice",
            Difficulty = "facile",
            PositionX = 100,
            PositionY = 200,
            BoundingBoxWidth = 300,
            BoundingBoxHeight = 400,
            ClassificationConfidence = 0.95,
            ExtractedDate = DateTime.UtcNow
        };

        await using (var write = NewContext())
        {
            var repository = new GameRepository(write);
            await repository.AddAsync(game);
            await repository.SaveChangesAsync();
        }

        await using var read = NewContext();
        var loaded = await new GameRepository(read).GetByIdAsync(game.Id);

        game.Id.Should().BeGreaterThan(0);
        loaded.Should().NotBeNull();
        loaded!.GameType.Should().Be(GameType.Crossword);
        loaded.Title.Should().Be("Cruciverba Facile");
        loaded.Difficulty.Should().Be("facile");
        loaded.PositionX.Should().Be(100);
        loaded.BoundingBoxHeight.Should().Be(400);
        loaded.ClassificationConfidence.Should().Be(0.95);
        loaded.HasSolution.Should().BeFalse();
    }

    [Fact]
    public async Task Games_of_a_page_are_ordered_by_vertical_then_horizontal_position()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[0], g => { g.Title = "basso"; g.PositionY = 500; g.PositionX = 0; });
        await SeedGameAsync(pageIds[0], g => { g.Title = "alto-destra"; g.PositionY = 10; g.PositionX = 300; });
        await SeedGameAsync(pageIds[0], g => { g.Title = "alto-sinistra"; g.PositionY = 10; g.PositionX = 5; });
        await SeedGameAsync(pageIds[1], g => g.Title = "altra pagina");

        await using var read = NewContext();
        var games = (await new GameRepository(read).GetByPageIdAsync(pageIds[0])).ToList();

        games.Select(g => g.Title).Should().Equal("alto-sinistra", "alto-destra", "basso");
    }

    [Fact]
    public async Task Games_of_a_type_are_returned_with_their_page_and_magazine()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        await SeedGameAsync(pageIds[0], g => { g.GameType = GameType.Rebus; g.Title = "rebus"; });
        await SeedGameAsync(pageIds[0], g => { g.GameType = GameType.Crossword; g.Title = "cruciverba"; });

        await using var read = NewContext();
        var games = (await new GameRepository(read).GetByGameTypeAsync(GameType.Rebus)).ToList();

        var game = games.Should().ContainSingle().Subject;
        game.Title.Should().Be("rebus");
        game.Page.Should().NotBeNull();
        game.Page.Magazine.Title.Should().Be("La Settimana Enigmistica");
    }

    [Fact]
    public async Task Games_of_a_magazine_are_ordered_by_page_and_leave_out_other_magazines()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        var (_, otherPageIds) = await SeedMagazineWithPagesAsync();
        await SeedGameAsync(pageIds[1], g => { g.Title = "pagina 2"; g.PositionY = 0; });
        await SeedGameAsync(pageIds[0], g => { g.Title = "pagina 1 sotto"; g.PositionY = 300; });
        await SeedGameAsync(pageIds[0], g => { g.Title = "pagina 1 sopra"; g.PositionY = 10; });
        await SeedGameAsync(otherPageIds[0], g => g.Title = "altra rivista");

        await using var read = NewContext();
        var games = (await new GameRepository(read).GetByMagazineIdAsync(magazineId)).ToList();

        games.Select(g => g.Title).Should().Equal("pagina 1 sopra", "pagina 1 sotto", "pagina 2");
    }

    [Fact]
    public async Task Game_with_details_loads_page_and_magazine()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        var gameId = await SeedGameAsync(pageIds[0]);

        await using var read = NewContext();
        var game = await new GameRepository(read).GetWithDetailsAsync(gameId);

        game.Should().NotBeNull();
        game!.Page.PageNumber.Should().Be(1);
        game.Page.Magazine.Year.Should().Be(2024);
    }
}

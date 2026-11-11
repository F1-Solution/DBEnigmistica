using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Tests.Integration;

[Collection(PostgresCollection.Name)]
[Trait("Category", "Integration")]
public class GameSolutionRepositoryIntegrationTests : IntegrationTestBase
{
    public GameSolutionRepositoryIntegrationTests(PostgresFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Linking_a_solution_stores_it_and_marks_the_game_as_solved()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        var gameId = await SeedGameAsync(pageIds[0]);

        await using (var write = NewContext())
        {
            var games = new GameRepository(write);
            var solutions = new GameSolutionRepository(write);

            var game = (await games.GetByIdAsync(gameId))!;
            game.Solution = "PRIMAVERA";
            game.SolutionPageId = pageIds[1];
            game.HasSolution = true;

            await solutions.AddAsync(NewSolution(gameId, "PRIMAVERA", verified: true, solutionPageId: pageIds[1]));
            await solutions.SaveChangesAsync();
        }

        await using var read = NewContext();
        var stored = (await new GameSolutionRepository(read).GetByGameIdAsync(gameId)).ToList();
        var reloaded = (await new GameRepository(read).GetByIdAsync(gameId))!;

        var solution = stored.Should().ContainSingle().Subject;
        solution.SolutionText.Should().Be("PRIMAVERA");
        solution.IsVerified.Should().BeTrue();
        solution.SolutionType.Should().Be(SolutionType.InSameIssue);
        solution.SolutionPage!.Id.Should().Be(pageIds[1]);
        reloaded.HasSolution.Should().BeTrue();
        reloaded.Solution.Should().Be("PRIMAVERA");
        reloaded.SolutionPageId.Should().Be(pageIds[1]);
    }

    [Fact]
    public async Task Solution_type_is_stored_as_text()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        var gameId = await SeedGameAsync(pageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.Add(NewSolution(gameId, type: SolutionType.InNextIssue));
            await write.SaveChangesAsync();
        }

        (await ScalarAsync<string>("SELECT \"SolutionType\" FROM riviste.game_solutions")).Should().Be("InNextIssue");
    }

    [Fact]
    public async Task Solution_of_a_missing_game_is_rejected_by_the_foreign_key()
    {
        await using var context = NewContext();
        context.GameSolutions.Add(NewSolution(gameId: 9999));

        var save = async () => await context.SaveChangesAsync();

        await save.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Unsolved_games_are_the_ones_without_a_solution_in_page_order()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[1], g => g.Title = "senza, pagina 2");
        await SeedGameAsync(pageIds[0], g => g.Title = "senza, pagina 1");
        await SeedGameAsync(pageIds[0], g => { g.Title = "risolto"; g.HasSolution = true; });

        await using var read = NewContext();
        var unsolved = (await new GameSolutionRepository(read).GetUnsolvedGamesAsync()).ToList();

        unsolved.Select(g => g.Title).Should().Equal("senza, pagina 1", "senza, pagina 2");
        unsolved.All(g => g.Page?.Magazine != null).Should().BeTrue();
    }

    [Fact]
    public async Task Unsolved_games_can_be_limited_to_one_magazine()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync();
        var (_, otherPageIds) = await SeedMagazineWithPagesAsync();
        await SeedGameAsync(pageIds[0], g => g.Title = "questa rivista");
        await SeedGameAsync(otherPageIds[0], g => g.Title = "altra rivista");

        await using var read = NewContext();
        var unsolved = (await new GameSolutionRepository(read).GetUnsolvedGamesAsync(magazineId)).ToList();

        unsolved.Select(g => g.Title).Should().Equal("questa rivista");
    }

    [Fact]
    public async Task Solutions_to_verify_are_the_unverified_ones_oldest_first()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        var gameId = await SeedGameAsync(pageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.AddRange(
                NewSolution(gameId, "recente", extractedDate: Utc(2024, 5, 1)),
                NewSolution(gameId, "verificata", verified: true, extractedDate: Utc(2024, 1, 1)),
                NewSolution(gameId, "vecchia", extractedDate: Utc(2024, 2, 1)));
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var pending = (await new GameSolutionRepository(read).GetPendingVerificationAsync()).ToList();

        pending.Select(s => s.SolutionText).Should().Equal("vecchia", "recente");
        pending[0].Game.Page.Magazine.Should().NotBeNull();
    }

    [Fact]
    public async Task Verified_solutions_can_be_limited_to_one_magazine()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync();
        var (_, otherPageIds) = await SeedMagazineWithPagesAsync();
        var gameId = await SeedGameAsync(pageIds[0]);
        var otherGameId = await SeedGameAsync(otherPageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.AddRange(
                NewSolution(gameId, "verificata qui", verified: true),
                NewSolution(gameId, "non verificata qui"),
                NewSolution(otherGameId, "verificata altrove", verified: true));
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var repository = new GameSolutionRepository(read);

        (await repository.GetVerifiedSolutionsAsync(magazineId)).Select(s => s.SolutionText)
            .Should().Equal("verificata qui");
        (await repository.GetVerifiedSolutionsAsync()).Select(s => s.SolutionText)
            .Should().BeEquivalentTo(new[] { "verificata qui", "verificata altrove" });
    }

    [Fact]
    public async Task Solutions_found_on_a_page_are_returned_with_their_game()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        var gameId = await SeedGameAsync(pageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.AddRange(
                NewSolution(gameId, "a pagina 2", solutionPageId: pageIds[1]),
                NewSolution(gameId, "senza pagina"));
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var found = (await new GameSolutionRepository(read).GetByPageIdAsync(pageIds[1])).ToList();

        found.Should().ContainSingle().Which.Game.Page.Id.Should().Be(pageIds[0]);
    }

    [Fact]
    public async Task Deleting_a_game_removes_its_solutions()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync();
        var gameId = await SeedGameAsync(pageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.Add(NewSolution(gameId));
            await write.SaveChangesAsync();
        }

        await using (var delete = NewContext())
        {
            await new GameRepository(delete).DeleteAsync(gameId);
            await delete.SaveChangesAsync();
        }

        (await ScalarAsync<long>("SELECT count(*) FROM riviste.game_solutions")).Should().Be(0);
    }

    [Fact]
    public async Task Deleting_a_page_that_is_the_solution_page_of_a_solution_is_rejected_and_nothing_changes()
    {
        var (_, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        var gameId = await SeedGameAsync(pageIds[0]);
        await using (var write = NewContext())
        {
            write.GameSolutions.Add(NewSolution(gameId, solutionPageId: pageIds[1]));
            await write.SaveChangesAsync();
        }

        await using (var delete = NewContext())
        {
            delete.Pages.Remove((await delete.Pages.FindAsync(pageIds[1]))!);
            var save = async () => await delete.SaveChangesAsync();
            await save.Should().ThrowAsync<DbUpdateException>();
        }

        await using var read = NewContext();
        (await read.GameSolutions.SingleAsync()).SolutionPageId.Should().Be(pageIds[1]);
        (await read.Pages.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task Deleting_a_magazine_whose_games_point_to_a_solution_page_of_the_same_magazine_succeeds()
    {
        var (magazineId, pageIds) = await SeedMagazineWithPagesAsync(pageCount: 2);
        await SeedGameAsync(pageIds[0], g => { g.SolutionPageId = pageIds[1]; g.HasSolution = true; });

        await using var write = NewContext();
        var repository = new MagazineRepository(write);
        await repository.DeleteAsync(magazineId);
        var save = async () => await repository.SaveChangesAsync();

        await save.Should().NotThrowAsync();
        (await ScalarAsync<long>("SELECT count(*) FROM riviste.games")).Should().Be(0);
    }
}

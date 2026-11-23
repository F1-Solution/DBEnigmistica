using DBEnigmistica.Domain.Diritti;
using DBEnigmistica.Application.Services.Solutions;
using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace DBEnigmistica.Tests.Unit.Cli;

public class GameSolutionServiceTests
{
    private readonly Mock<IGameRepository> _games = new();
    private readonly Mock<IGameSolutionRepository> _solutions = new();

    private GameSolutionService Service() =>
        new(_games.Object, _solutions.Object, NullLogger<GameSolutionService>.Instance);

    [Fact]
    public async Task Link_MarksGameAsSolvedAndSavesAVerifiedSolution()
    {
        var game = new Game { Origine = Origine.Rivista, Tipologia = "cruciverba-classico" };
        _games.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(game);
        _solutions.Setup(r => r.AddAsync(It.IsAny<GameSolution>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GameSolution s, CancellationToken _) => s);

        var linked = await Service().LinkAsync(7, "ROMA", 3, SolutionType.InNextIssue);

        game.HasSolution.Should().BeTrue();
        game.Solution.Should().Be("ROMA");
        game.SolutionPageId.Should().Be(3);
        linked.GameId.Should().Be(7);
        linked.IsVerified.Should().BeTrue();
        linked.SolutionType.Should().Be(SolutionType.InNextIssue);
        _solutions.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Link_Throws_WhenGameDoesNotExist()
    {
        _games.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Game?)null);

        var act = () => Service().LinkAsync(1, "x", null, SolutionType.External);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Gioco 1*");
    }

    [Fact]
    public async Task Verify_Approved_PropagatesSolutionToGame()
    {
        var game = new Game { Origine = Origine.Rivista, Tipologia = "cruciverba-classico" };
        var solution = new GameSolution { Id = 5, GameId = 2, SolutionText = "ROMA", SolutionPageId = 9 };
        _solutions.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(solution);
        _games.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(game);

        await Service().VerifyAsync(5, true, "ok");

        solution.IsVerified.Should().BeTrue();
        solution.Notes.Should().Be("ok");
        game.Solution.Should().Be("ROMA");
        game.HasSolution.Should().BeTrue();
        _solutions.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Verify_Rejected_LeavesTheGameUntouched()
    {
        var solution = new GameSolution { Id = 5, GameId = 2, IsVerified = true };
        _solutions.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(solution);

        await Service().VerifyAsync(5, false, null);

        solution.IsVerified.Should().BeFalse();
        _games.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Verify_Throws_WhenSolutionDoesNotExist()
    {
        _solutions.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((GameSolution?)null);

        var act = () => Service().VerifyAsync(1, true, null);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*Soluzione 1*");
    }
}

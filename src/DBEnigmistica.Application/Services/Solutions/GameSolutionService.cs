using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DBEnigmistica.Application.Services.Solutions;

public class GameSolutionService : IGameSolutionService
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameSolutionRepository _solutionRepository;
    private readonly ILogger<GameSolutionService> _logger;

    public GameSolutionService(
        IGameRepository gameRepository,
        IGameSolutionRepository solutionRepository,
        ILogger<GameSolutionService> logger)
    {
        _gameRepository = gameRepository;
        _solutionRepository = solutionRepository;
        _logger = logger;
    }

    public async Task<GameSolution> LinkAsync(
        int gameId,
        string solutionText,
        int? pageId,
        SolutionType solutionType,
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId)
            ?? throw new KeyNotFoundException($"Gioco {gameId} non trovato");

        game.Solution = solutionText;
        game.SolutionPageId = pageId;
        game.HasSolution = true;

        var now = DateTime.UtcNow;
        var gameSolution = new GameSolution
        {
            GameId = gameId,
            SolutionText = solutionText,
            SolutionPageId = pageId,
            SolutionType = solutionType,
            IsVerified = true,
            ExtractedDate = now,
            VerifiedDate = now
        };

        await _solutionRepository.AddAsync(gameSolution, cancellationToken);
        await _solutionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Linked solution {SolutionId} to game {GameId}", gameSolution.Id, gameId);
        return gameSolution;
    }

    public async Task<GameSolution> VerifyAsync(
        int solutionId,
        bool verified,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        var gameSolution = await _solutionRepository.GetByIdAsync(solutionId)
            ?? throw new KeyNotFoundException($"Soluzione {solutionId} non trovata");

        gameSolution.IsVerified = verified;
        gameSolution.VerifiedDate = DateTime.UtcNow;
        if (notes != null)
        {
            gameSolution.Notes = notes;
        }

        if (verified)
        {
            await PropagateToGameAsync(gameSolution);
        }

        await _solutionRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Solution {SolutionId} verified={Verified}", solutionId, verified);
        return gameSolution;
    }

    private async Task PropagateToGameAsync(GameSolution gameSolution)
    {
        var game = await _gameRepository.GetByIdAsync(gameSolution.GameId);
        if (game == null)
        {
            return;
        }

        game.Solution = gameSolution.SolutionText;
        game.SolutionPageId = gameSolution.SolutionPageId;
        game.HasSolution = true;
    }
}

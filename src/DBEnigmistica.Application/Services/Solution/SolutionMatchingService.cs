using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Enums;
using DBEnigmistica.Domain.Interfaces;

namespace DBEnigmistica.Application.Services.Solution;

public class SolutionMatchingService : ISolutionMatchingService
{
    private readonly IGameSolutionRepository _solutionRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IRepository<Page> _pageRepository;
    private readonly IAiGameAnalyzer _aiAnalyzer;

    public SolutionMatchingService(
        IGameSolutionRepository solutionRepository,
        IGameRepository gameRepository,
        IRepository<Page> pageRepository,
        IAiGameAnalyzer aiAnalyzer)
    {
        _solutionRepository = solutionRepository;
        _gameRepository = gameRepository;
        _pageRepository = pageRepository;
        _aiAnalyzer = aiAnalyzer;
    }

    public async Task<IEnumerable<SolutionMatch>> FindPotentialSolutionsAsync(
        int pageId, 
        CancellationToken cancellationToken = default)
    {
        var page = await _pageRepository.GetByIdAsync(pageId);
        if (page == null)
        {
            return Enumerable.Empty<SolutionMatch>();
        }

        var unsolvedGames = await _solutionRepository.GetUnsolvedGamesAsync(page.MagazineId);
        
        return Enumerable.Empty<SolutionMatch>();
    }

    public async Task<int> LinkSolutionToGameAsync(
        int gameId, 
        string solutionText, 
        int? solutionPageId, 
        SolutionType solutionType, 
        double? confidence = null, 
        CancellationToken cancellationToken = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new ArgumentException($"Game with ID {gameId} not found", nameof(gameId));
        }

        game.Solution = solutionText;
        game.SolutionPageId = solutionPageId;
        game.HasSolution = true;

        var gameSolution = new GameSolution
        {
            GameId = gameId,
            SolutionText = solutionText,
            SolutionPageId = solutionPageId,
            SolutionType = solutionType,
            ConfidenceScore = confidence,
            IsVerified = confidence == null,
            ExtractedDate = DateTime.UtcNow,
            VerifiedDate = confidence == null ? DateTime.UtcNow : null
        };

        await _solutionRepository.AddAsync(gameSolution, cancellationToken);
        await _solutionRepository.SaveChangesAsync(cancellationToken);

        return gameSolution.Id;
    }

    public async Task VerifySolutionAsync(
        int solutionId, 
        bool isCorrect, 
        string? notes = null, 
        CancellationToken cancellationToken = default)
    {
        var solution = await _solutionRepository.GetByIdAsync(solutionId);
        if (solution == null)
        {
            throw new ArgumentException($"Solution with ID {solutionId} not found", nameof(solutionId));
        }

        solution.IsVerified = isCorrect;
        solution.VerifiedDate = DateTime.UtcNow;
        if (notes != null)
        {
            solution.Notes = notes;
        }

        if (isCorrect)
        {
            var game = await _gameRepository.GetByIdAsync(solution.GameId);
            if (game != null)
            {
                game.Solution = solution.SolutionText;
                game.SolutionPageId = solution.SolutionPageId;
                game.HasSolution = true;
            }
        }

        await _solutionRepository.SaveChangesAsync(cancellationToken);
    }
}
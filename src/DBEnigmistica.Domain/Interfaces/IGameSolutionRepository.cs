using DBEnigmistica.Domain.Entities;

namespace DBEnigmistica.Domain.Interfaces;

public interface IGameSolutionRepository : IRepository<GameSolution>
{
    Task<IEnumerable<GameSolution>> GetByGameIdAsync(int gameId);
    Task<IEnumerable<Game>> GetUnsolvedGamesAsync(int? magazineId = null);
    Task<IEnumerable<GameSolution>> GetByPageIdAsync(int pageId);
    Task<IEnumerable<GameSolution>> GetPendingVerificationAsync();
    Task<IEnumerable<GameSolution>> GetVerifiedSolutionsAsync(int? magazineId = null);
}

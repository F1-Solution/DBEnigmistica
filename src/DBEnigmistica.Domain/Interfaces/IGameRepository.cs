using DBEnigmistica.Domain.Entities;

namespace DBEnigmistica.Domain.Interfaces;

public interface IGameRepository : IRepository<Game>
{
    Task<IEnumerable<Game>> GetByPageIdAsync(int pageId);
    Task<IEnumerable<Game>> GetByGameTypeAsync(Enums.GameType gameType);
    Task<IEnumerable<Game>> GetByMagazineIdAsync(int magazineId);
    Task<Game?> GetWithDetailsAsync(int id);
}

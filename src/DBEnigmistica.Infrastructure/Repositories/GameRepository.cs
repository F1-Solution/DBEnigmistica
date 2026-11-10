using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

public class GameRepository : Repository<Game>, IGameRepository
{
    public GameRepository(EnigmisticaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Game>> GetByPageIdAsync(int pageId)
    {
        return await _dbSet
            .Where(g => g.PageId == pageId)
            .OrderBy(g => g.PositionY)
            .ThenBy(g => g.PositionX)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByGameTypeAsync(Domain.Enums.GameType gameType)
    {
        return await _dbSet
            .Where(g => g.GameType == gameType)
            .Include(g => g.Page)
                .ThenInclude(p => p.Magazine)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByMagazineIdAsync(int magazineId)
    {
        return await _dbSet
            .Include(g => g.Page)
            .Where(g => g.Page.MagazineId == magazineId)
            .OrderBy(g => g.Page.PageNumber)
            .ThenBy(g => g.PositionY)
            .ToListAsync();
    }

    public async Task<Game?> GetWithDetailsAsync(int id)
    {
        return await _dbSet
            .Include(g => g.Page)
                .ThenInclude(p => p.Magazine)
            .FirstOrDefaultAsync(g => g.Id == id);
    }
}

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
            .OrderBy(g => g.RiquadroY)
            .ThenBy(g => g.RiquadroX)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetByTipologiaAsync(string tipologia)
    {
        return await _dbSet
            .Where(g => g.Tipologia == tipologia)
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
            .ThenBy(g => g.RiquadroY)
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

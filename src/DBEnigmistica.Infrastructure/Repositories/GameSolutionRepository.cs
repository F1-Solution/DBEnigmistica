using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

public class GameSolutionRepository : Repository<GameSolution>, IGameSolutionRepository
{
    public GameSolutionRepository(EnigmisticaDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<GameSolution>> GetByGameIdAsync(int gameId)
    {
        return await _dbSet
            .Where(gs => gs.GameId == gameId)
            .Include(gs => gs.SolutionPage)
            .OrderBy(gs => gs.ExtractedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Game>> GetUnsolvedGamesAsync(int? magazineId = null)
    {
        var query = _context.Set<Game>()
            .Where(g => !g.HasSolution)
            .Include(g => g.Page)
                .ThenInclude(p => p.Magazine)
            .AsQueryable();

        if (magazineId.HasValue)
        {
            query = query.Where(g => g.Page.MagazineId == magazineId.Value);
        }

        return await query
            .OrderBy(g => g.Page.MagazineId)
            .ThenBy(g => g.Page.PageNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameSolution>> GetByPageIdAsync(int pageId)
    {
        return await _dbSet
            .Where(gs => gs.SolutionPageId == pageId)
            .Include(gs => gs.Game)
                .ThenInclude(g => g.Page)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameSolution>> GetPendingVerificationAsync()
    {
        return await _dbSet
            .Where(gs => !gs.IsVerified)
            .Include(gs => gs.Game)
                .ThenInclude(g => g.Page)
                    .ThenInclude(p => p.Magazine)
            .Include(gs => gs.SolutionPage)
            .OrderBy(gs => gs.ExtractedDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<GameSolution>> GetVerifiedSolutionsAsync(int? magazineId = null)
    {
        var query = _dbSet
            .Where(gs => gs.IsVerified)
            .Include(gs => gs.Game)
                .ThenInclude(g => g.Page)
                    .ThenInclude(p => p.Magazine)
            .Include(gs => gs.SolutionPage)
            .AsQueryable();

        if (magazineId.HasValue)
        {
            query = query.Where(gs => gs.Game.Page.MagazineId == magazineId.Value);
        }

        return await query.ToListAsync();
    }
}

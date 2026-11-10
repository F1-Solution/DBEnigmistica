using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

public class MagazineRepository : Repository<Magazine>, IMagazineRepository
{
    public MagazineRepository(EnigmisticaDbContext context) : base(context)
    {
    }

    public async Task<Magazine?> GetByTitleAndDateAsync(string title, DateTime publicationDate)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.Title == title && m.PublicationDate.Date == publicationDate.Date);
    }

    public async Task<IEnumerable<Magazine>> GetByYearAsync(int year)
    {
        return await _dbSet
            .Where(m => m.Year == year)
            .OrderBy(m => m.PublicationDate)
            .ToListAsync();
    }

    public async Task<Magazine?> GetWithPagesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Pages)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Magazine?> GetWithPagesAndGamesAsync(int id)
    {
        return await _dbSet
            .Include(m => m.Pages)
                .ThenInclude(p => p.Games)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddPageAsync(Page page, CancellationToken cancellationToken = default)
    {
        await _context.Set<Page>().AddAsync(page, cancellationToken);
    }
}

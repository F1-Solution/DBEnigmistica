using DBEnigmistica.Domain.Entities;

namespace DBEnigmistica.Domain.Interfaces;

public interface IMagazineRepository : IRepository<Magazine>
{
    Task<Magazine?> GetByTitleAndDateAsync(string title, DateTime publicationDate);
    Task<IEnumerable<Magazine>> GetByYearAsync(int year);
    Task<Magazine?> GetWithPagesAsync(int id);
    Task<Magazine?> GetWithPagesAndGamesAsync(int id);
    Task AddPageAsync(Page page, CancellationToken cancellationToken = default);
}

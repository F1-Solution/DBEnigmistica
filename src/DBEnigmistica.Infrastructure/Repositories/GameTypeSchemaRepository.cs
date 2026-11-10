using DBEnigmistica.Domain.Entities;
using DBEnigmistica.Domain.Interfaces;
using DBEnigmistica.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DBEnigmistica.Infrastructure.Repositories;

public class GameTypeSchemaRepository : Repository<GameTypeSchema>, IGameTypeSchemaRepository
{
    public GameTypeSchemaRepository(EnigmisticaDbContext context) : base(context)
    {
    }

    public async Task<GameTypeSchema?> GetByGameTypeNameAsync(string gameTypeName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.GameTypeName.ToLower() == gameTypeName.ToLower());
    }

    public async Task<GameTypeSchema?> GetByTableNameAsync(string tableName)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.TableName.ToLower() == tableName.ToLower());
    }

    public async Task<IEnumerable<GameTypeSchema>> GetAllValidatedAsync()
    {
        return await _dbSet
            .Where(s => s.IsUserValidated)
            .OrderBy(s => s.CreatedDate)
            .ToListAsync();
    }

    public async Task<bool> TableNameExistsAsync(string tableName)
    {
        return await _dbSet
            .AnyAsync(s => s.TableName.ToLower() == tableName.ToLower());
    }
}

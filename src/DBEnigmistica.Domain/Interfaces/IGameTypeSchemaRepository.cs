using DBEnigmistica.Domain.Entities;

namespace DBEnigmistica.Domain.Interfaces;

public interface IGameTypeSchemaRepository : IRepository<GameTypeSchema>
{
    Task<GameTypeSchema?> GetByGameTypeNameAsync(string gameTypeName);
    Task<GameTypeSchema?> GetByTableNameAsync(string tableName);
    Task<IEnumerable<GameTypeSchema>> GetAllValidatedAsync();
    Task<bool> TableNameExistsAsync(string tableName);
}

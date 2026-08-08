using CicdPractice.Api.Entities;

namespace CicdPractice.Api.Repositories;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(int id);
    Task<Player?> GetByJerseyNumberAsync(int jerseyNumber);
    Task<List<Player>> GetAllAsync();
    Task<Player> AddAsync(Player player);
    Task RemoveAsync(Player player);
    Task SaveChangesAsync();
}

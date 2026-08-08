using CicdPractice.Api.Entities;
using CicdPractice.Api.Models;

namespace CicdPractice.Api.Managers;

public interface IPlayerManager
{
    Task<List<Player>> GetAllPlayersAsync();
    Task<Player> AddPlayerAsync(CreatePlayerRequest request);
    Task RemovePlayerAsync(int playerId);
    Task<List<Player>> EditLineupAsync(EditLineupRequest request);
}

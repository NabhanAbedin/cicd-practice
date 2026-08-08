using CicdPractice.Api.Entities;
using CicdPractice.Api.Exceptions;
using CicdPractice.Api.Models;
using CicdPractice.Api.Repositories;

namespace CicdPractice.Api.Managers;

public class PlayerManager : IPlayerManager
{
    private const int RequiredStartingPlayers = 11;

    private readonly IPlayerRepository _playerRepository;

    public PlayerManager(IPlayerRepository playerRepository)
    {
        _playerRepository = playerRepository;
    }

    public async Task<List<Player>> GetAllPlayersAsync() =>
        await _playerRepository.GetAllAsync();

    public async Task<Player> AddPlayerAsync(CreatePlayerRequest request)
    {
        var existing = await _playerRepository.GetByJerseyNumberAsync(request.JerseyNumber);
        if (existing is not null)
        {
            throw new DuplicateJerseyNumberException(
                $"Jersey number {request.JerseyNumber} is already assigned to another player.");
        }

        var player = new Player
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            JerseyNumber = request.JerseyNumber,
            Position = request.Position,
            LineupStatus = LineupStatus.Reserve
        };

        return await _playerRepository.AddAsync(player);
    }

    public async Task RemovePlayerAsync(int playerId)
    {
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player is null)
        {
            throw new PlayerNotFoundException($"Player with id {playerId} was not found.");
        }

        await _playerRepository.RemoveAsync(player);
    }

    public async Task<List<Player>> EditLineupAsync(EditLineupRequest request)
    {
        var startingIds = request.StartingPlayerIds.Distinct().ToList();
        if (startingIds.Count != RequiredStartingPlayers)
        {
            throw new InvalidLineupException(
                $"A starting lineup must have exactly {RequiredStartingPlayers} distinct players, but {startingIds.Count} were provided.");
        }

        var allPlayers = await _playerRepository.GetAllAsync();
        var allPlayerIds = allPlayers.Select(p => p.Id).ToHashSet();

        var missingIds = startingIds.Where(id => !allPlayerIds.Contains(id)).ToList();
        if (missingIds.Count > 0)
        {
            throw new PlayerNotFoundException(
                $"Player(s) not found: {string.Join(", ", missingIds)}");
        }

        var startingIdSet = startingIds.ToHashSet();
        foreach (var player in allPlayers)
        {
            var newStatus = startingIdSet.Contains(player.Id) ? LineupStatus.Starting : LineupStatus.Reserve;
            if (player.LineupStatus != newStatus)
            {
                player.LineupStatus = newStatus;
                player.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _playerRepository.SaveChangesAsync();

        return allPlayers.Where(p => p.LineupStatus == LineupStatus.Starting).ToList();
    }
}

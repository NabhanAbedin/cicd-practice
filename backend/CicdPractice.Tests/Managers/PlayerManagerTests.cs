using CicdPractice.Api.Entities;
using CicdPractice.Api.Exceptions;
using CicdPractice.Api.Managers;
using CicdPractice.Api.Models;
using CicdPractice.Api.Repositories;
using Moq;

namespace CicdPractice.Tests.Managers;

public class PlayerManagerTests
{
    private readonly Mock<IPlayerRepository> _repository = new();
    private readonly PlayerManager _manager;

    public PlayerManagerTests()
    {
        _manager = new PlayerManager(_repository.Object);
    }

    private static Player CreatePlayer(int id, int jerseyNumber, LineupStatus status = LineupStatus.Reserve) =>
        new()
        {
            Id = id,
            FirstName = "First" + id,
            LastName = "Last" + id,
            JerseyNumber = jerseyNumber,
            Position = PlayerPosition.Midfielder,
            LineupStatus = status
        };

    [Fact]
    public async Task AddPlayerAsync_JerseyNumberAvailable_AddsPlayerWithReserveStatus()
    {
        var request = new CreatePlayerRequest
        {
            FirstName = "Lionel",
            LastName = "Messi",
            JerseyNumber = 10,
            Position = PlayerPosition.Forward
        };

        _repository.Setup(r => r.GetByJerseyNumberAsync(10)).ReturnsAsync((Player?)null);
        _repository.Setup(r => r.AddAsync(It.IsAny<Player>()))
            .ReturnsAsync((Player p) =>
            {
                p.Id = 1;
                return p;
            });

        var result = await _manager.AddPlayerAsync(request);

        Assert.Equal(1, result.Id);
        Assert.Equal("Lionel", result.FirstName);
        Assert.Equal(LineupStatus.Reserve, result.LineupStatus);
        _repository.Verify(r => r.AddAsync(It.Is<Player>(p =>
            p.FirstName == "Lionel" &&
            p.JerseyNumber == 10 &&
            p.LineupStatus == LineupStatus.Reserve)), Times.Once);
    }

    [Fact]
    public async Task AddPlayerAsync_JerseyNumberTaken_ThrowsDuplicateJerseyNumberException()
    {
        var request = new CreatePlayerRequest { FirstName = "New", LastName = "Guy", JerseyNumber = 10, Position = PlayerPosition.Defender };
        _repository.Setup(r => r.GetByJerseyNumberAsync(10)).ReturnsAsync(CreatePlayer(1, 10));

        await Assert.ThrowsAsync<DuplicateJerseyNumberException>(() => _manager.AddPlayerAsync(request));

        _repository.Verify(r => r.AddAsync(It.IsAny<Player>()), Times.Never);
    }

    [Fact]
    public async Task RemovePlayerAsync_PlayerExists_RemovesPlayer()
    {
        var player = CreatePlayer(1, 7);
        _repository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(player);

        await _manager.RemovePlayerAsync(1);

        _repository.Verify(r => r.RemoveAsync(player), Times.Once);
    }

    [Fact]
    public async Task RemovePlayerAsync_PlayerDoesNotExist_ThrowsPlayerNotFoundException()
    {
        _repository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Player?)null);

        await Assert.ThrowsAsync<PlayerNotFoundException>(() => _manager.RemovePlayerAsync(99));

        _repository.Verify(r => r.RemoveAsync(It.IsAny<Player>()), Times.Never);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public async Task EditLineupAsync_NotExactlyElevenDistinctIds_ThrowsInvalidLineupException(int count)
    {
        var request = new EditLineupRequest { StartingPlayerIds = Enumerable.Range(1, count).ToList() };

        await Assert.ThrowsAsync<InvalidLineupException>(() => _manager.EditLineupAsync(request));

        _repository.Verify(r => r.GetAllAsync(), Times.Never);
        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EditLineupAsync_DuplicateIdsCollapseBelowEleven_ThrowsInvalidLineupException()
    {
        // 12 raw ids but only 10 distinct values
        var ids = Enumerable.Range(1, 10).ToList();
        ids.AddRange([1, 2]);
        var request = new EditLineupRequest { StartingPlayerIds = ids };

        await Assert.ThrowsAsync<InvalidLineupException>(() => _manager.EditLineupAsync(request));
    }

    [Fact]
    public async Task EditLineupAsync_ContainsUnknownPlayerId_ThrowsPlayerNotFoundException()
    {
        var existingPlayers = Enumerable.Range(1, 10).Select(id => CreatePlayer(id, id)).ToList();
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingPlayers);

        var request = new EditLineupRequest
        {
            StartingPlayerIds = Enumerable.Range(1, 10).Append(999).ToList()
        };

        await Assert.ThrowsAsync<PlayerNotFoundException>(() => _manager.EditLineupAsync(request));

        _repository.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task EditLineupAsync_ValidElevenPlayers_SetsThoseToStartingAndRestToReserve()
    {
        // 15 players total; the first 5 already Starting, the rest Reserve.
        var existingPlayers = Enumerable.Range(1, 15)
            .Select(id => CreatePlayer(id, id, id <= 5 ? LineupStatus.Starting : LineupStatus.Reserve))
            .ToList();
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(existingPlayers);

        var newStartingIds = Enumerable.Range(5, 11).ToList(); // ids 5..15
        var request = new EditLineupRequest { StartingPlayerIds = newStartingIds };

        var result = await _manager.EditLineupAsync(request);

        Assert.Equal(11, result.Count);
        Assert.All(result, p => Assert.Contains(p.Id, newStartingIds));

        foreach (var player in existingPlayers)
        {
            var expectedStatus = newStartingIds.Contains(player.Id) ? LineupStatus.Starting : LineupStatus.Reserve;
            Assert.Equal(expectedStatus, player.LineupStatus);
        }

        // Player 1 was Starting before and is now benched.
        Assert.Equal(LineupStatus.Reserve, existingPlayers.Single(p => p.Id == 1).LineupStatus);

        _repository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllPlayersAsync_ReturnsPlayersFromRepository()
    {
        var players = new List<Player> { CreatePlayer(1, 1), CreatePlayer(2, 2) };
        _repository.Setup(r => r.GetAllAsync()).ReturnsAsync(players);

        var result = await _manager.GetAllPlayersAsync();

        Assert.Same(players, result);
    }
}

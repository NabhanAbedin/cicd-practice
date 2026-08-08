using CicdPractice.Api.Controllers;
using CicdPractice.Api.Entities;
using CicdPractice.Api.Exceptions;
using CicdPractice.Api.Managers;
using CicdPractice.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CicdPractice.Tests.Controllers;

public class PlayersControllerTests
{
    private readonly Mock<IPlayerManager> _manager = new();
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _controller = new PlayersController(_manager.Object);
    }

    private static Player CreatePlayer(int id, LineupStatus status = LineupStatus.Reserve) =>
        new()
        {
            Id = id,
            FirstName = "First" + id,
            LastName = "Last" + id,
            JerseyNumber = id,
            Position = PlayerPosition.Midfielder,
            LineupStatus = status
        };

    [Fact]
    public async Task GetAllPlayers_ReturnsOkWithPlayers()
    {
        var players = new List<Player> { CreatePlayer(1), CreatePlayer(2) };
        _manager.Setup(m => m.GetAllPlayersAsync()).ReturnsAsync(players);

        var result = await _controller.GetAllPlayers();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(players, okResult.Value);
    }

    [Fact]
    public async Task AddPlayer_ManagerSucceeds_ReturnsCreatedAtActionWithPlayer()
    {
        var request = new CreatePlayerRequest { FirstName = "Lionel", LastName = "Messi", JerseyNumber = 10, Position = PlayerPosition.Forward };
        var created = CreatePlayer(1);
        _manager.Setup(m => m.AddPlayerAsync(request)).ReturnsAsync(created);

        var result = await _controller.AddPlayer(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Same(created, createdResult.Value);
    }

    [Fact]
    public async Task AddPlayer_DuplicateJerseyNumber_ReturnsConflict()
    {
        var request = new CreatePlayerRequest { FirstName = "New", LastName = "Guy", JerseyNumber = 10, Position = PlayerPosition.Defender };
        _manager.Setup(m => m.AddPlayerAsync(request))
            .ThrowsAsync(new DuplicateJerseyNumberException("Jersey number 10 is already assigned to another player."));

        var result = await _controller.AddPlayer(request);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RemovePlayer_PlayerExists_ReturnsNoContent()
    {
        _manager.Setup(m => m.RemovePlayerAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.RemovePlayer(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemovePlayer_PlayerDoesNotExist_ReturnsNotFound()
    {
        _manager.Setup(m => m.RemovePlayerAsync(99))
            .ThrowsAsync(new PlayerNotFoundException("Player with id 99 was not found."));

        var result = await _controller.RemovePlayer(99);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task EditLineup_ManagerSucceeds_ReturnsOkWithStartingLineup()
    {
        var request = new EditLineupRequest { StartingPlayerIds = Enumerable.Range(1, 11).ToList() };
        var startingLineup = Enumerable.Range(1, 11).Select(id => CreatePlayer(id, LineupStatus.Starting)).ToList();
        _manager.Setup(m => m.EditLineupAsync(request)).ReturnsAsync(startingLineup);

        var result = await _controller.EditLineup(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Same(startingLineup, okResult.Value);
    }

    [Fact]
    public async Task EditLineup_InvalidLineup_ReturnsBadRequest()
    {
        var request = new EditLineupRequest { StartingPlayerIds = [1, 2, 3] };
        _manager.Setup(m => m.EditLineupAsync(request))
            .ThrowsAsync(new InvalidLineupException("A starting lineup must have exactly 11 distinct players, but 3 were provided."));

        var result = await _controller.EditLineup(request);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EditLineup_UnknownPlayerId_ReturnsNotFound()
    {
        var request = new EditLineupRequest { StartingPlayerIds = Enumerable.Range(1, 11).ToList() };
        _manager.Setup(m => m.EditLineupAsync(request))
            .ThrowsAsync(new PlayerNotFoundException("Player(s) not found: 999"));

        var result = await _controller.EditLineup(request);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}

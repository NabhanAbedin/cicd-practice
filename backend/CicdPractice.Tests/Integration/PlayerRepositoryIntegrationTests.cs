using CicdPractice.Api.Data;
using CicdPractice.Api.Entities;
using CicdPractice.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CicdPractice.Tests.Integration;

public class PlayerRepositoryIntegrationTests : IClassFixture<PostgresContainerFixture>, IAsyncLifetime
{
    private readonly PostgresContainerFixture _fixture;
    private FootballManagerDbContext _context = null!;
    private PlayerRepository _repository = null!;

    public PlayerRepositoryIntegrationTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        _context = _fixture.CreateContext();
        _repository = new PlayerRepository(_context);

        // Start each test with an empty table so tests don't interfere with each other.
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Players\"");
    }

    public async Task DisposeAsync() => await _context.DisposeAsync();

    private static Player NewPlayer(int jerseyNumber, string firstName = "First", string lastName = "Last") =>
        new()
        {
            FirstName = firstName,
            LastName = lastName,
            JerseyNumber = jerseyNumber,
            Position = PlayerPosition.Midfielder
        };

    [Fact]
    public async Task AddAsync_PersistsPlayerAndAssignsGeneratedId()
    {
        var added = await _repository.AddAsync(NewPlayer(10));

        Assert.True(added.Id > 0);

        await using var freshContext = _fixture.CreateContext();
        var reloaded = await freshContext.Players.FindAsync(added.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(10, reloaded!.JerseyNumber);
    }

    [Fact]
    public async Task AddAsync_DuplicateJerseyNumber_ThrowsBecauseOfUniqueIndex()
    {
        await _repository.AddAsync(NewPlayer(7));

        await Assert.ThrowsAsync<DbUpdateException>(() => _repository.AddAsync(NewPlayer(7)));
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(999_999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByJerseyNumberAsync_FindsMatchingPlayer()
    {
        await _repository.AddAsync(NewPlayer(9, "Karim", "Benzema"));

        var found = await _repository.GetByJerseyNumberAsync(9);

        Assert.NotNull(found);
        Assert.Equal("Karim", found!.FirstName);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEveryPersistedPlayer()
    {
        await _repository.AddAsync(NewPlayer(1));
        await _repository.AddAsync(NewPlayer(2));

        var all = await _repository.GetAllAsync();

        Assert.Equal(2, all.Count);
    }

    [Fact]
    public async Task RemoveAsync_DeletesPlayerFromDatabase()
    {
        var player = await _repository.AddAsync(NewPlayer(5));

        await _repository.RemoveAsync(player);

        await using var freshContext = _fixture.CreateContext();
        var reloaded = await freshContext.Players.FindAsync(player.Id);
        Assert.Null(reloaded);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsInPlaceEdits()
    {
        var player = await _repository.AddAsync(NewPlayer(11));

        player.LineupStatus = LineupStatus.Starting;
        await _repository.SaveChangesAsync();

        await using var freshContext = _fixture.CreateContext();
        var reloaded = await freshContext.Players.FindAsync(player.Id);
        Assert.Equal(LineupStatus.Starting, reloaded!.LineupStatus);
    }
}

using CicdPractice.Api.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CicdPractice.Tests.Integration;

public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("footballManagerTestDb")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public FootballManagerDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FootballManagerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new FootballManagerDbContext(options);
    }
}

using CicdPractice.Api.Data;
using CicdPractice.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CicdPractice.Api.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly FootballManagerDbContext _context;

    public PlayerRepository(FootballManagerDbContext context)
    {
        _context = context;
    }

    public async Task<Player?> GetByIdAsync(int id) =>
        await _context.Players.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Player?> GetByJerseyNumberAsync(int jerseyNumber) =>
        await _context.Players.FirstOrDefaultAsync(p => p.JerseyNumber == jerseyNumber);

    public async Task<List<Player>> GetAllAsync() =>
        await _context.Players.ToListAsync();

    public async Task<Player> AddAsync(Player player)
    {
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task RemoveAsync(Player player)
    {
        _context.Players.Remove(player);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}

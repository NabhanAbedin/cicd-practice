using CicdPractice.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CicdPractice.Api.Data;

public class FootballManagerDbContext : DbContext
{
    public FootballManagerDbContext(DbContextOptions<FootballManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Players");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Position).HasConversion<string>().HasMaxLength(20);
            entity.Property(p => p.LineupStatus).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(p => p.JerseyNumber).IsUnique();
        });
    }
}

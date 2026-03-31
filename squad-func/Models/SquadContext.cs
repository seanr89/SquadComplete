using Microsoft.EntityFrameworkCore;

namespace squad_func.Models;

public class SquadContext : DbContext
{
    public SquadContext(DbContextOptions<SquadContext> options) : base(options) { }

    public DbSet<League> Leagues { get; set; }
    public DbSet<Fixture> Fixtures { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerFixtureStatistic> PlayerFixtureStatistics { get; set; }
    public DbSet<GameRecord> GameRecords { get; set; }
    public DbSet<GameRecordTag> GameRecordTags { get; set; }
    public DbSet<Formation> Formations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration for decimal precision on Rating
        modelBuilder.Entity<PlayerFixtureStatistic>()
            .Property(p => p.Rating)
            .HasPrecision(4, 2);

        // Unique constraint for GameRecordTag
        modelBuilder.Entity<GameRecordTag>()
            .HasIndex(t => new { t.GameRecordId, t.FixtureId, t.TeamId })
            .IsUnique();
    }
}

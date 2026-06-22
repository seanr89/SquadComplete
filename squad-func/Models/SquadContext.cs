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
    public DbSet<Season> Seasons { get; set; }
    public DbSet<TeamSeason> TeamSeasons { get; set; }
    public DbSet<UserSquad> UserSquads { get; set; }
    public DbSet<UserSquadPlayer> UserSquadPlayers { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<Feedback> Feedback { get; set; }

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

        // Unique constraint for TeamSeason
        modelBuilder.Entity<TeamSeason>()
            .HasIndex(t => new { t.TeamId, t.SeasonId })
            .IsUnique();

        // Unique constraint for User BrowserIdentifierId
        modelBuilder.Entity<User>()
            .HasIndex(u => u.BrowserIdentifierId)
            .IsUnique();

        // Unique constraint for UserSquad (user_id, game_record_id)
        modelBuilder.Entity<UserSquad>()
            .HasIndex(us => new { us.UserId, us.GameRecordId })
            .IsUnique();

        // Unique constraint for UserSquadPlayer (user_squad_id, player_id)
        modelBuilder.Entity<UserSquadPlayer>()
            .HasIndex(usp => new { usp.UserSquadId, usp.PlayerId })
            .IsUnique();
    }
}

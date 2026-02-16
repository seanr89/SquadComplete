using Microsoft.EntityFrameworkCore;

namespace squad_api.Models;

public class SquadContext : DbContext
{
    public SquadContext(DbContextOptions<SquadContext> options) : base(options) { }

    public DbSet<League> Leagues { get; set; }
    public DbSet<Fixture> Fixtures { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<PlayerFixtureStatistic> PlayerFixtureStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Ensure standard behavior or specific configurations if needed
        // For example, if we wanted snake_case mapping globally without attributes, we'd use snake case naming convention package.
        // But we used [Column("name")] attributes so manual mapping is done.
    }
}

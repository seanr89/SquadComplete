using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_api.Models;

[Table("fixtures")]
public class Fixture
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Column("league_id")]
    public int? LeagueId { get; set; }

    [ForeignKey("LeagueId")]
    public League? League { get; set; }

    [Column("home_team_id")]
    public int? HomeTeamId { get; set; }

    [Column("home_team_name")]
    [StringLength(255)]
    public string? HomeTeamName { get; set; }

    [Column("away_team_id")]
    public int? AwayTeamId { get; set; }

    [Column("away_team_name")]
    [StringLength(255)]
    public string? AwayTeamName { get; set; }

    [Column("home_goal_count")]
    public int? HomeGoalCount { get; set; }

    [Column("away_goal_count")]
    public int? AwayGoalCount { get; set; }

    [Column("fixture_date")]
    public DateTime? FixtureDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

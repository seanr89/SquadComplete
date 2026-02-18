using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("player_fixture_statistics")]
[PrimaryKey(nameof(FixtureId), nameof(PlayerId))]
public class PlayerFixtureStatistic
{
    [Column("fixture_id")]
    public int FixtureId { get; set; }

    [ForeignKey("FixtureId")]
    public Fixture? Fixture { get; set; }

    [Column("team_id")]
    public int? TeamId { get; set; }

    [ForeignKey("TeamId")]
    public Team? Team { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [ForeignKey("PlayerId")]
    public Player? Player { get; set; }

    [Column("minutes")]
    public int? Minutes { get; set; }

    [Column("number")]
    public int? Number { get; set; }

    [Column("position")]
    [StringLength(50)]
    public string? Position { get; set; }

    [Column("rating")]
    public decimal? Rating { get; set; }

    [Column("is_captain")]
    public bool IsCaptain { get; set; } = false;

    [Column("is_substitute")]
    public bool IsSubstitute { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

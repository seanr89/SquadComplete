using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("game_record_tags")]
public class GameRecordTag
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("game_record_id")]
    public int GameRecordId { get; set; }

    [ForeignKey("GameRecordId")]
    public GameRecord GameRecord { get; set; } = null!;

    [Column("fixture_id")]
    public int FixtureId { get; set; }

    [ForeignKey("FixtureId")]
    public Fixture Fixture { get; set; } = null!;

    [Column("team_id")]
    public int TeamId { get; set; }

    [ForeignKey("TeamId")]
    public Team Team { get; set; } = null!;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

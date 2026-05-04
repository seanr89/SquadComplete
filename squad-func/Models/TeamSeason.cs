using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("team_seasons")]
public class TeamSeason
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("team_id")]
    public int TeamId { get; set; }

    [Column("season_id")]
    public int? SeasonId { get; set; }

    [Column("data_requested")]
    public bool DataRequested { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("TeamId")]
    public Team Team { get; set; } = null!;

    [ForeignKey("SeasonId")]
    public Season? Season { get; set; }
}

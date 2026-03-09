using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using squad_func.Models;

namespace squad_api.Models;

[Table("user_squad_players")]
public class UserSquadPlayer
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_squad_id")]
    public int UserSquadId { get; set; }

    [ForeignKey(nameof(UserSquadId))]
    public UserSquad? UserSquad { get; set; }

    [Column("player_id")]
    public int PlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public Player? Player { get; set; }

    [Column("is_captain")]
    public bool IsCaptain { get; set; }

    [Column("is_vice_captain")]
    public bool IsViceCaptain { get; set; }

    [MaxLength(50)]
    [Column("position")]
    public string? Position { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

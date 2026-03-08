using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("user_squads")]
public class UserSquad
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [Column("game_record_id")]
    public int GameRecordId { get; set; }

    [ForeignKey(nameof(GameRecordId))]
    public GameRecord? GameRecord { get; set; }

    [Column("formation_id")]
    public int? FormationId { get; set; }

    [ForeignKey(nameof(FormationId))]
    public Formation? Formation { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserSquadPlayer> UserSquadPlayers { get; set; } = new List<UserSquadPlayer>();
}

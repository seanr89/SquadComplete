using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("game_records")]
public class GameRecord
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column("game_date")]
    public DateTime GameDate { get; set; }

    [Column("formation_id")]
    public int? FormationId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Formation? Formation { get; set; }

    public ICollection<GameRecordTag> Tags { get; set; } = new List<GameRecordTag>();
}

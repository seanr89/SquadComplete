using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_api.Models;

[Table("teams")]
public class Team
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("logo")]
    public string? Logo { get; set; }

    [Column("last_update")]
    public DateTime? LastUpdate { get; set; }

    [Column("active")]
    public bool Active { get; set; } = false;

    [Column("source_location")]
    [StringLength(255)]
    public string? SourceLocation { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

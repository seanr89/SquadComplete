using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_func.Models;

[Table("formations")]
public class Formation
{
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("defence")]
    public int Defence { get; set; }

    [Column("midfield")]
    public int Midfield { get; set; }

    [Column("attack")]
    public int Attack { get; set; }
}

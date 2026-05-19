using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_domain.Models;

[Table("leagues")]
public class League
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Column("type")]
    [StringLength(50)]
    public string? Type { get; set; }

    [Column("logo")]
    public string? Logo { get; set; }

    [Column("country_name")]
    [StringLength(100)]
    public string? CountryName { get; set; }

    [Column("country_code")]
    [StringLength(10)]
    public string? CountryCode { get; set; }

    [Column("country_flag")]
    public string? CountryFlag { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("api_id")]
    public int? ApiId { get; set; }
}

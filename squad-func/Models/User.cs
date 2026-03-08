using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace squad_api.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("browser_identifier_id")]
    public string BrowserIdentifierId { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("name")]
    public string? Name { get; set; }

    [MaxLength(255)]
    [Column("email")]
    public string? Email { get; set; }
}

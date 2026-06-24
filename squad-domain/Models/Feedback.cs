using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace squad_domain.Models;

[Table("feedback")]
public class Feedback
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Column("name")]
    public string Name { get; set; } = null!;
    [Column("email")]
    public string Email { get; set; } = null!;
    [Column("message")]
    public string Message { get; set; } = null!;
    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

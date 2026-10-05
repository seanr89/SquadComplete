namespace squad_api.DTOs;

public class PlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public int? ApiId { get; set; }
}

public record PlayerResponse(
    int Id,
    string Name,
    string? Photo,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ApiId)
{
    public static PlayerResponse From(Player p) => new(p.Id, p.Name, p.Photo, p.CreatedAt, p.UpdatedAt, p.ApiId);
}

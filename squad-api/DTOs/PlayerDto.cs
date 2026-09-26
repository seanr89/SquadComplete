namespace squad_api.DTOs;

public class PlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string? Photo { get; set; }
    public int? ApiId { get; set; }
}

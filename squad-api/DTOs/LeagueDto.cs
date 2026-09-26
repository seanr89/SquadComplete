namespace squad_api.DTOs;

public class LeagueDto
{
    public string Name { get; set; } = string.Empty;
    public string? Type { get; set; }
    public string? Logo { get; set; }
    public string? CountryName { get; set; }
    public string? CountryCode { get; set; }
    public string? CountryFlag { get; set; }
    public int? ApiId { get; set; }
}

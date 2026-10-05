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

public record LeagueResponse(
    int Id,
    string Name,
    string? Type,
    string? Logo,
    string? CountryName,
    string? CountryCode,
    string? CountryFlag,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ApiId)
{
    public static LeagueResponse From(League l) => new(
        l.Id, l.Name, l.Type, l.Logo, l.CountryName, l.CountryCode, l.CountryFlag, l.CreatedAt, l.UpdatedAt, l.ApiId);
}

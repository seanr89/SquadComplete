namespace squad_api.DTOs;

public record TeamResponse(
    int Id,
    string Name,
    string? Logo,
    DateTime? LastUpdate,
    bool Active,
    string? SourceLocation,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ApiId)
{
    public static TeamResponse From(Team t) => new(
        t.Id, t.Name, t.Logo, t.LastUpdate, t.Active, t.SourceLocation, t.CreatedAt, t.UpdatedAt, t.ApiId);
}

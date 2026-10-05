namespace squad_api.DTOs;

public class FixtureDto
{
    public int? LeagueId { get; set; }
    public int? HomeTeamId { get; set; }
    public string? HomeTeamName { get; set; }
    public int? AwayTeamId { get; set; }
    public string? AwayTeamName { get; set; }
    public int? HomeGoalCount { get; set; }
    public int? AwayGoalCount { get; set; }
    public DateTime? FixtureDate { get; set; }
    public string? FixtureSource { get; set; }
    public int? ApiId { get; set; }
}

public record FixtureResponse(
    int Id,
    int? LeagueId,
    LeagueResponse? League,
    int? HomeTeamId,
    string? HomeTeamName,
    int? AwayTeamId,
    string? AwayTeamName,
    int? HomeGoalCount,
    int? AwayGoalCount,
    DateTime? FixtureDate,
    string? FixtureSource,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int? ApiId)
{
    /// <summary><see cref="League"/> is populated only when the entity was loaded with its league.</summary>
    public static FixtureResponse From(Fixture f) => new(
        f.Id, f.LeagueId, f.League is null ? null : LeagueResponse.From(f.League), f.HomeTeamId, f.HomeTeamName,
        f.AwayTeamId, f.AwayTeamName, f.HomeGoalCount, f.AwayGoalCount, f.FixtureDate, f.FixtureSource,
        f.CreatedAt, f.UpdatedAt, f.ApiId);
}

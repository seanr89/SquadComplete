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

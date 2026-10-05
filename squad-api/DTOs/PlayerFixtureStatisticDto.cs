namespace squad_api.DTOs;

public class PlayerFixtureStatisticDto
{
    public int FixtureId { get; set; }
    public int PlayerId { get; set; }
    public int? TeamId { get; set; }
    public int? Minutes { get; set; }
    public int? Number { get; set; }
    public string? Position { get; set; }
    public decimal? Rating { get; set; }
    public bool IsCaptain { get; set; }
    public bool IsSubstitute { get; set; }
}

public record PlayerFixtureStatisticResponse(
    int FixtureId,
    FixtureResponse? Fixture,
    int? TeamId,
    TeamResponse? Team,
    int PlayerId,
    PlayerResponse? Player,
    int? Minutes,
    int? Number,
    string? Position,
    decimal? Rating,
    bool IsCaptain,
    bool IsSubstitute,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    /// <summary>Related <c>Fixture</c>/<c>Team</c>/<c>Player</c> are populated only when the entity was loaded with them.</summary>
    public static PlayerFixtureStatisticResponse From(PlayerFixtureStatistic s) => new(
        s.FixtureId, s.Fixture is null ? null : FixtureResponse.From(s.Fixture),
        s.TeamId, s.Team is null ? null : TeamResponse.From(s.Team),
        s.PlayerId, s.Player is null ? null : PlayerResponse.From(s.Player),
        s.Minutes, s.Number, s.Position, s.Rating, s.IsCaptain, s.IsSubstitute, s.CreatedAt, s.UpdatedAt);
}

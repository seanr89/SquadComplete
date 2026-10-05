namespace squad_api.DTOs;

/// <summary>One submitted squad on a game record's leaderboard.</summary>
/// <param name="Id">The user squad id, as a string (the frontend keys rows by string id).</param>
public record LeaderboardEntryDto(
    string Id,
    string PlayerName,
    decimal TeamAverageRating,
    IReadOnlyList<LeaderboardPlayerDto> Squad);

/// <param name="Id">The player id, as a string.</param>
public record LeaderboardPlayerDto(string? Id, string? Name, string? Position, decimal Rating);

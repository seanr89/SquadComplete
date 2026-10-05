using System;
using System.Collections.Generic;
using squad_api.Models;

namespace squad_api.DTOs;

public class GameRecordDto
{
    public int Id { get; set; }
    public DateTime GameDate { get; set; }
    public FormationResponse Formation { get; set; } = new(0, "Unknown", 0, 0, 0);
    public List<GameRecordTeamDto> Teams { get; set; } = new();
}

public class GameRecordTeamDto
{
    public int? FixtureId { get; set; }
    public int? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogo { get; set; }
    public string Formation { get; set; } = string.Empty;
    public List<GameRecordPlayerDto> Players { get; set; } = new();
}

public class GameRecordPlayerDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? PlayerPhoto { get; set; }
    public GameRecordPlayerStatisticDto? Statistics { get; set; }
}

public class GameRecordPlayerStatisticDto
{
    public string? Position { get; set; }
    public decimal? Rating { get; set; }
}

public record GameRecordTagResponse(
    int Id,
    int GameRecordId,
    int FixtureId,
    int TeamId,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static GameRecordTagResponse From(GameRecordTag t) => new(
        t.Id, t.GameRecordId, t.FixtureId, t.TeamId, t.CreatedAt, t.UpdatedAt);
}

/// <summary>The stored game record row and its tags, as returned when a record is deleted.</summary>
public record GameRecordResponse(
    int Id,
    DateTime GameDate,
    int? FormationId,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<GameRecordTagResponse> Tags)
{
    public static GameRecordResponse From(GameRecord r) => new(
        r.Id, r.GameDate, r.FormationId, r.CreatedAt, r.UpdatedAt, r.Tags.Select(GameRecordTagResponse.From).ToList());
}

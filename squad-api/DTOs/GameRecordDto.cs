using System;
using System.Collections.Generic;

namespace squad_api.DTOs;

public class GameRecordDto
{
    public int Id { get; set; }
    public DateTime GameDate { get; set; }
    //public DateTime CreatedAt { get; set; }
    //public DateTime UpdatedAt { get; set; }
    public List<GameRecordTeamDto> Teams { get; set; } = new();
}

public class GameRecordTeamDto
{
    // public int FixtureId { get; set; }
    // public int TeamId { get; set; }
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
    // public int? Minutes { get; set; }
    // public int? Number { get; set; }
    public string? Position { get; set; }
    public decimal? Rating { get; set; }
    // public bool IsCaptain { get; set; }
    // public bool IsSubstitute { get; set; }
}

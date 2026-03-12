using System;
using System.Collections.Generic;
using squad_api.Models;

namespace squad_api.DTOs;

public class GameRecordDto
{
    public int Id { get; set; }
    public DateTime GameDate { get; set; }
    public Formation Formation { get; set; } = new();
    public List<GameRecordTeamDto> Teams { get; set; } = new();
}

public class GameRecordTeamDto
{
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

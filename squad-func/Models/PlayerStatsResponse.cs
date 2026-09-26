using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace squad_func.Models;

public class PlayerStatsResponse
{
    [JsonPropertyName("team")]
    public PlayerStatsTeam? Team { get; set; }

    [JsonPropertyName("players")]
    public List<PlayerStatsData>? Players { get; set; }
}

public class PlayerStatsTeam
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("update")]
    public DateTime? Update { get; set; }
}

public class PlayerStatsData
{
    [JsonPropertyName("player")]
    public PlayerStatsPlayerInfo? Player { get; set; }

    [JsonPropertyName("statistics")]
    public List<PlayerStatsStatistic>? Statistics { get; set; }
}

public class PlayerStatsPlayerInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("photo")]
    public string? Photo { get; set; }
}

public class PlayerStatsStatistic
{
    [JsonPropertyName("games")]
    public PlayerStatsGames? Games { get; set; }
}

public class PlayerStatsGames
{
    [JsonPropertyName("minutes")]
    public int? Minutes { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("rating")]
    public string? Rating { get; set; }

    [JsonPropertyName("captain")]
    public bool? Captain { get; set; }

    [JsonPropertyName("substitute")]
    public bool? Substitute { get; set; }
}

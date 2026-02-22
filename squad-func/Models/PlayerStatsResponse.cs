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

    // [JsonPropertyName("offsides")]
    // public int? Offsides { get; set; }

    // [JsonPropertyName("shots")]
    // public PlayerStatsShots? Shots { get; set; }

    // [JsonPropertyName("goals")]
    // public PlayerStatsGoals? Goals { get; set; }

    // [JsonPropertyName("passes")]
    // public PlayerStatsPasses? Passes { get; set; }

    // [JsonPropertyName("tackles")]
    // public PlayerStatsTackles? Tackles { get; set; }

    // [JsonPropertyName("duels")]
    // public PlayerStatsDuels? Duels { get; set; }

    // [JsonPropertyName("dribbles")]
    // public PlayerStatsDribbles? Dribbles { get; set; }

    // [JsonPropertyName("fouls")]
    // public PlayerStatsFouls? Fouls { get; set; }

    // [JsonPropertyName("cards")]
    // public PlayerStatsCards? Cards { get; set; }

    // [JsonPropertyName("penalty")]
    // public PlayerStatsPenalty? Penalty { get; set; }
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

// public class PlayerStatsShots
// {
//     [JsonPropertyName("total")]
//     public int? Total { get; set; }

//     [JsonPropertyName("on")]
//     public int? On { get; set; }
// }

// public class PlayerStatsGoals
// {
//     [JsonPropertyName("total")]
//     public int? Total { get; set; }

//     [JsonPropertyName("conceded")]
//     public int? Conceded { get; set; }

//     [JsonPropertyName("assists")]
//     public int? Assists { get; set; }

//     [JsonPropertyName("saves")]
//     public int? Saves { get; set; }
// }

// public class PlayerStatsPasses
// {
//     [JsonPropertyName("total")]
//     public int? Total { get; set; }

//     [JsonPropertyName("key")]
//     public int? Key { get; set; }

//     [JsonPropertyName("accuracy")]
//     public string? Accuracy { get; set; }
// }

// public class PlayerStatsTackles
// {
//     [JsonPropertyName("total")]
//     public int? Total { get; set; }

//     [JsonPropertyName("blocks")]
//     public int? Blocks { get; set; }

//     [JsonPropertyName("interceptions")]
//     public int? Interceptions { get; set; }
// }

// public class PlayerStatsDuels
// {
//     [JsonPropertyName("total")]
//     public int? Total { get; set; }

//     [JsonPropertyName("won")]
//     public int? Won { get; set; }
// }

// public class PlayerStatsDribbles
// {
//     [JsonPropertyName("attempts")]
//     public int? Attempts { get; set; }

//     [JsonPropertyName("success")]
//     public int? Success { get; set; }

//     [JsonPropertyName("past")]
//     public int? Past { get; set; }
// }

// public class PlayerStatsFouls
// {
//     [JsonPropertyName("drawn")]
//     public int? Drawn { get; set; }

//     [JsonPropertyName("committed")]
//     public int? Committed { get; set; }
// }

// public class PlayerStatsCards
// {
//     [JsonPropertyName("yellow")]
//     public int? Yellow { get; set; }

//     [JsonPropertyName("red")]
//     public int? Red { get; set; }
// }

// public class PlayerStatsPenalty
// {
//     [JsonPropertyName("won")]
//     public int? Won { get; set; }

//     [JsonPropertyName("commited")]
//     public int? Commited { get; set; } // Matches the typo in the API response

//     [JsonPropertyName("scored")]
//     public int? Scored { get; set; }

//     [JsonPropertyName("missed")]
//     public int? Missed { get; set; }

//     [JsonPropertyName("saved")]
//     public int? Saved { get; set; }
// }

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace squad_func.Models;

public class FixtureApiResponse
{
    [JsonPropertyName("fixture")]
    public ApiFixtureInfo? Fixture { get; set; }

    [JsonPropertyName("league")]
    public ApiLeagueInfo? League { get; set; }

    [JsonPropertyName("teams")]
    public ApiTeamsInfo? Teams { get; set; }

    [JsonPropertyName("goals")]
    public ApiGoalsInfo? Goals { get; set; }

    [JsonPropertyName("score")]
    public ApiScoreInfo? Score { get; set; }

    [JsonPropertyName("events")]
    public List<ApiEventInfo>? Events { get; set; }

    [JsonPropertyName("lineups")]
    public List<ApiLineupInfo>? Lineups { get; set; }

    [JsonPropertyName("statistics")]
    public List<ApiStatisticsInfo>? Statistics { get; set; }
}

public class ApiFixtureInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("referee")]
    public string? Referee { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("date")]
    public DateTime? Date { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("periods")]
    public ApiPeriodsInfo? Periods { get; set; }

    [JsonPropertyName("venue")]
    public ApiVenueInfo? Venue { get; set; }

    [JsonPropertyName("status")]
    public ApiStatusInfo? Status { get; set; }
}

public class ApiPeriodsInfo
{
    [JsonPropertyName("first")]
    public long? First { get; set; }

    [JsonPropertyName("second")]
    public long? Second { get; set; }
}

public class ApiVenueInfo
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }
}

public class ApiStatusInfo
{
    [JsonPropertyName("long")]
    public string? Long { get; set; }

    [JsonPropertyName("short")]
    public string? Short { get; set; }

    [JsonPropertyName("elapsed")]
    public int? Elapsed { get; set; }
}

public class ApiLeagueInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }

    [JsonPropertyName("season")]
    public int? Season { get; set; }

    [JsonPropertyName("round")]
    public string? Round { get; set; }
}

public class ApiTeamsInfo
{
    [JsonPropertyName("home")]
    public ApiTeamDetail? Home { get; set; }

    [JsonPropertyName("away")]
    public ApiTeamDetail? Away { get; set; }
}

public class ApiTeamDetail
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }

    [JsonPropertyName("winner")]
    public bool? Winner { get; set; }
}

public class ApiGoalsInfo
{
    [JsonPropertyName("home")]
    public int? Home { get; set; }

    [JsonPropertyName("away")]
    public int? Away { get; set; }
}

public class ApiScoreInfo
{
    [JsonPropertyName("halftime")]
    public ApiGoalsInfo? Halftime { get; set; }

    [JsonPropertyName("fulltime")]
    public ApiGoalsInfo? Fulltime { get; set; }

    [JsonPropertyName("extratime")]
    public ApiGoalsInfo? Extratime { get; set; }

    [JsonPropertyName("penalty")]
    public ApiGoalsInfo? Penalty { get; set; }
}

public class ApiEventInfo
{
    [JsonPropertyName("time")]
    public ApiEventTime? Time { get; set; }

    [JsonPropertyName("team")]
    public ApiTeamDetail? Team { get; set; }

    [JsonPropertyName("player")]
    public ApiEventPlayer? Player { get; set; }

    [JsonPropertyName("assist")]
    public ApiEventPlayer? Assist { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("comments")]
    public string? Comments { get; set; }
}

public class ApiEventTime
{
    [JsonPropertyName("elapsed")]
    public int? Elapsed { get; set; }

    [JsonPropertyName("extra")]
    public int? Extra { get; set; }
}

public class ApiEventPlayer
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class ApiLineupInfo
{
    [JsonPropertyName("team")]
    public ApiLineupTeam? Team { get; set; }

    [JsonPropertyName("coach")]
    public ApiCoachInfo? Coach { get; set; }

    [JsonPropertyName("formation")]
    public string? Formation { get; set; }

    [JsonPropertyName("startXI")]
    public List<ApiLineupPlayerWrapper>? StartXI { get; set; }

    [JsonPropertyName("substitutes")]
    public List<ApiLineupPlayerWrapper>? Substitutes { get; set; }
}

public class ApiLineupTeam
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }
}

public class ApiCoachInfo
{
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class ApiLineupPlayerWrapper
{
    [JsonPropertyName("player")]
    public ApiLineupPlayer? Player { get; set; }
}

public class ApiLineupPlayer
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }

    [JsonPropertyName("pos")]
    public string? Pos { get; set; }

    [JsonPropertyName("grid")]
    public string? Grid { get; set; }
}

public class ApiStatisticsInfo
{
    [JsonPropertyName("team")]
    public ApiTeamDetail? Team { get; set; }

    [JsonPropertyName("statistics")]
    public List<ApiStatisticDetail>? Statistics { get; set; }
}

public class ApiStatisticDetail
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }
}

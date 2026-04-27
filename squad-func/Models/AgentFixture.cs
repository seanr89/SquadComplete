using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace squad_func.Models;

public class AgentFixture
{
    [JsonPropertyName("league")]
    public string? League { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("matches")]
    public List<AgentMatch>? Matches { get; set; }
}

public class AgentMatch
{
    [JsonPropertyName("fixture")]
    public AgentMatchDetails? Fixture { get; set; }

    [JsonPropertyName("score")]
    public AgentScore? Score { get; set; }

    [JsonPropertyName("lineups")]
    public AgentLineups? Lineups { get; set; }
}

public class AgentMatchDetails
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("time")]
    public string? Time { get; set; }

    [JsonPropertyName("home_team")]
    public string? HomeTeam { get; set; }

    [JsonPropertyName("away_team")]
    public string? AwayTeam { get; set; }
}

public class AgentScore
{
    [JsonPropertyName("home_score")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? HomeScore { get; set; }

    [JsonPropertyName("away_score")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? AwayScore { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class AgentLineups
{
    [JsonPropertyName("home_starting_xi")]
    public List<string>? HomeStartingXi { get; set; }

    [JsonPropertyName("away_starting_xi")]
    public List<string>? AwayStartingXi { get; set; }
}

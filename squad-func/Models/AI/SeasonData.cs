using System.Text.Json.Serialization;

public class SeasonData
{
    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("season")]
    public string? Season { get; set; }

    [JsonPropertyName("fixtures")]
    public List<HistoricalFixture>? Fixtures { get; set; }
}

public class HistoricalFixture
{
    [JsonPropertyName("home_team")]
    public string? HomeTeam { get; set; }

    [JsonPropertyName("away_team")]
    public string? AwayTeam { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("score")]
    public string? Score { get; set; }
}
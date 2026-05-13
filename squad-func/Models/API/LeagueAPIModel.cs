using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Squad.Function.Models.API;

public class LeagueAPIModel
{
    [JsonPropertyName("get")]
    public string? Get { get; set; }

    [JsonPropertyName("parameters")]
    public Dictionary<string, string>? Parameters { get; set; }

    [JsonPropertyName("errors")]
    public List<object>? Errors { get; set; }

    [JsonPropertyName("results")]
    public int Results { get; set; }

    [JsonPropertyName("paging")]
    public PagingData? Paging { get; set; }

    [JsonPropertyName("response")]
    public List<LeagueResponseItem>? Response { get; set; }
}

public class PagingData
{
    [JsonPropertyName("current")]
    public int Current { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }
}

public class LeagueResponseItem
{
    [JsonPropertyName("league")]
    public LeagueData? League { get; set; }

    [JsonPropertyName("country")]
    public CountryData? Country { get; set; }
}

public class LeagueData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("logo")]
    public string? Logo { get; set; }
}

public class CountryData
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("flag")]
    public string? Flag { get; set; }
}

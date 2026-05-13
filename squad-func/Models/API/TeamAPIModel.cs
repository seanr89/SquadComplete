using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Squad.Function.Models.API;

public record TeamAPIModel(
    [property: JsonPropertyName("get")] string? Get,
    [property: JsonPropertyName("parameters")] Dictionary<string, string>? Parameters,
    [property: JsonPropertyName("errors")] List<object>? Errors,
    [property: JsonPropertyName("results")] int Results,
    [property: JsonPropertyName("paging")] PagingData? Paging,
    [property: JsonPropertyName("response")] List<TeamResponseItem>? Response
);

public record TeamResponseItem(
    [property: JsonPropertyName("team")] TeamData? Team
);

public record TeamData(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("country")] string? Country,
    [property: JsonPropertyName("founded")] int? Founded,
    [property: JsonPropertyName("national")] bool National,
    [property: JsonPropertyName("logo")] string? Logo
);

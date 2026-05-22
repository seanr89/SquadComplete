using System.Text.Json.Serialization;
using System.Collections.Generic;
using squad_func.Models;

namespace Squad.Function.Models.API;

public record MappedPlayer(Player dbPlayer, PlayerAPIModel? apiPlayer, Squad.Function.Models.AI.PlayerData? filePlayerData);

public record PlayerAPIModel(
    [property: JsonPropertyName("get")] string? Get,
    [property: JsonPropertyName("parameters")] Dictionary<string, string>? Parameters,
    [property: JsonPropertyName("errors")] object? Errors,
    [property: JsonPropertyName("results")] int Results,
    [property: JsonPropertyName("paging")] PagingData? Paging,
    [property: JsonPropertyName("response")] List<PlayerResponseItem>? Response
);

public record PlayerResponseItem(
    [property: JsonPropertyName("player")] PlayerData? Player
);

public record PlayerData(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string? Name,
    [property: JsonPropertyName("firstname")] string? Firstname,
    [property: JsonPropertyName("lastname")] string? Lastname,
    [property: JsonPropertyName("age")] int? Age,
    [property: JsonPropertyName("birth")] BirthData? Birth,
    [property: JsonPropertyName("nationality")] string? Nationality,
    [property: JsonPropertyName("height")] string? Height,
    [property: JsonPropertyName("weight")] string? Weight,
    [property: JsonPropertyName("number")] int? Number,
    [property: JsonPropertyName("position")] string? Position,
    [property: JsonPropertyName("photo")] string? Photo
);

public record BirthData(
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("place")] string? Place,
    [property: JsonPropertyName("country")] string? Country
);

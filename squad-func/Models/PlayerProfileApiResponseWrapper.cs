using System.Text.Json.Serialization;

namespace squad_func.Models;

public class PlayerProfileApiResponseWrapper
{
    [JsonPropertyName("player")]
    public PlayerStatsPlayerInfo? Player { get; set; }
}

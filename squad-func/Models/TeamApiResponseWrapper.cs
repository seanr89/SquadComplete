using System.Text.Json.Serialization;

namespace squad_func.Models;

public class TeamApiResponseWrapper
{
    [JsonPropertyName("team")]
    public ApiTeamDetail? Team { get; set; }
}

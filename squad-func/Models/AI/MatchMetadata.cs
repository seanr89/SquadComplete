using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Squad.Function.Models.AI
{
    public class MatchDetails
    {
        [JsonPropertyName("match_metadata")]
        public MatchMetadata MatchMetadata { get; set; }

        [JsonPropertyName("home_team")]
        public TeamData HomeTeam { get; set; }

        [JsonPropertyName("away_team")]
        public TeamData AwayTeam { get; set; }
    }

    public class MatchMetadata
    {
        [JsonPropertyName("fixture")]
        public string Fixture { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("competition")]
        public string Competition { get; set; }

        [JsonPropertyName("final_score")]
        public string FinalScore { get; set; }
    }

    public class TeamData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("players")]
        public List<PlayerData> Players { get; set; }
    }

    public class PlayerData
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("position")]
        public string Position { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("is_starter")]
        public bool IsStarter { get; set; }
    }
}

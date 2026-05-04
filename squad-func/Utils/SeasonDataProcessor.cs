
using System.Text.Json;

public static class SeasonDataProcessor
{
    public static string? ProcessAndRemoveLosses(string responseString, string team)
    {
        using JsonDocument doc = JsonDocument.Parse(responseString);
        var root = doc.RootElement;
        var textResponse = root
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (textResponse != null)
        {
            textResponse = textResponse.Trim();
            if (textResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
            {
                textResponse = textResponse.Substring(7);
            }
            else if (textResponse.StartsWith("```"))
            {
                textResponse = textResponse.Substring(3);
            }

            if (textResponse.EndsWith("```"))
            {
                textResponse = textResponse.Substring(0, textResponse.Length - 3);
            }

            textResponse = textResponse.Trim();

            SeasonData? seasonData = null;
            try
            {
                seasonData = JsonSerializer.Deserialize<SeasonData>(textResponse);
            }
            catch (JsonException)
            {
                Console.WriteLine("Error: The output format does not meet the expected JSON format.");
                return null;
            }

            if (seasonData == null || seasonData.Fixtures == null)
            {
                Console.WriteLine("Error: The output format does not meet the expected JSON format.");
                return null;
            }

            Console.WriteLine("Response received successfully and matches expected format.");

            var fixturesToRemove = new List<HistoricalFixture>();
            foreach (var fixture in seasonData.Fixtures)
            {
                if (string.IsNullOrEmpty(fixture.Score) || string.IsNullOrEmpty(fixture.HomeTeam) || string.IsNullOrEmpty(fixture.AwayTeam))
                    continue;

                var scores = fixture.Score.Split('-');
                if (scores.Length == 2 && int.TryParse(scores[0].Trim(), out int homeScore) && int.TryParse(scores[1].Trim(), out int awayScore))
                {
                    bool isHome = fixture.HomeTeam.Contains(team, StringComparison.OrdinalIgnoreCase);
                    bool isAway = fixture.AwayTeam.Contains(team, StringComparison.OrdinalIgnoreCase);

                    if (isHome && homeScore < awayScore)
                    {
                        fixturesToRemove.Add(fixture);
                    }
                    else if (isAway && awayScore < homeScore)
                    {
                        fixturesToRemove.Add(fixture);
                    }
                    else if (!isHome && !isAway)
                    {
                        // Fallback to the team returned in JSON
                        isHome = !string.IsNullOrEmpty(seasonData.Team) && fixture.HomeTeam.Contains(seasonData.Team, StringComparison.OrdinalIgnoreCase);
                        isAway = !string.IsNullOrEmpty(seasonData.Team) && fixture.AwayTeam.Contains(seasonData.Team, StringComparison.OrdinalIgnoreCase);

                        if (isHome && homeScore < awayScore)
                            fixturesToRemove.Add(fixture);
                        else if (isAway && awayScore < homeScore)
                            fixturesToRemove.Add(fixture);
                    }
                }
            }

            foreach (var f in fixturesToRemove)
            {
                seasonData.Fixtures.Remove(f);
            }

            return JsonSerializer.Serialize(seasonData, new JsonSerializerOptions { WriteIndented = true });
        }

        return null;
    }
}

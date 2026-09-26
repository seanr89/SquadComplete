using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Squad.Function.Models.AI;

namespace Squad.Function.Utils;

/// <summary>
/// Validation and sanitisation for Gemini-produced match data before it is written to the database.
/// AI output is untrusted: values are trimmed, stripped of control/invisible characters, whitespace-collapsed,
/// truncated to the target column's length, and numeric values are clamped to sane ranges.
/// </summary>
public static class AiDataSanitizer
{
    // Column lengths from squad-domain [StringLength] attributes.
    public const int NameMaxLength = 255;      // Player/Team/League name, Fixture home/away team name
    public const int PositionMaxLength = 50;   // PlayerFixtureStatistic.Position

    public const decimal MinRating = 0.0m;
    public const decimal MaxRating = 10.0m;
    public const int MaxGoals = 99;

    /// <summary>
    /// Cleans free text from AI output. Returns null if nothing meaningful remains.
    /// Unicode letters (accented/non-Latin names) are preserved; only control, format, private-use
    /// and surrogate characters are removed.
    /// </summary>
    public static string? CleanText(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var sb = new StringBuilder(value.Length);
        bool lastWasSpace = false;
        foreach (var ch in value.Normalize(NormalizationForm.FormC))
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!lastWasSpace && sb.Length > 0)
                {
                    sb.Append(' ');
                }
                lastWasSpace = true;
                continue;
            }

            var category = char.GetUnicodeCategory(ch);
            if (category is UnicodeCategory.Control or UnicodeCategory.Format
                or UnicodeCategory.PrivateUse or UnicodeCategory.Surrogate or UnicodeCategory.OtherNotAssigned)
            {
                continue;
            }

            sb.Append(ch);
            lastWasSpace = false;
        }

        var cleaned = sb.ToString().TrimEnd();
        if (cleaned.Length > maxLength)
        {
            cleaned = cleaned[..maxLength].TrimEnd();
        }

        return cleaned.Length == 0 ? null : cleaned;
    }

    /// <summary>
    /// Converts an AI rating to the stored decimal, rounded to 1dp (DB is NUMERIC(3,1)).
    /// Missing, non-finite or out-of-range (0-10) ratings become 0.
    /// </summary>
    public static decimal CleanRating(double? rating)
    {
        if (rating is not double r || double.IsNaN(r) || double.IsInfinity(r))
        {
            return 0.0m;
        }

        var value = Math.Round((decimal)r, 1, MidpointRounding.AwayFromZero);
        return value is < MinRating or > MaxRating ? 0.0m : value;
    }

    /// <summary>
    /// Parses a "H-A" score string. Returns false if it isn't two integers in 0-99.
    /// </summary>
    public static bool TryParseScore(string? finalScore, out int homeGoals, out int awayGoals)
    {
        homeGoals = 0;
        awayGoals = 0;

        var parts = finalScore?.Split('-', StringSplitOptions.TrimEntries);
        if (parts is not { Length: 2 }
            || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var home)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var away)
            || home > MaxGoals || away > MaxGoals)
        {
            return false;
        }

        homeGoals = home;
        awayGoals = away;
        return true;
    }

    /// <summary>
    /// Sanitises the match payload in place and returns a list of validation errors.
    /// An empty list means the data is safe to ingest. Players whose names are empty after
    /// cleaning are dropped (the downstream 11-player check still guards squad completeness).
    /// </summary>
    public static List<string> SanitizeAndValidate(MatchDetails? matchData)
    {
        var errors = new List<string>();
        if (matchData == null)
        {
            errors.Add("Match data could not be deserialised.");
            return errors;
        }

        if (matchData.MatchMetadata == null)
        {
            errors.Add("Missing match_metadata.");
        }
        else
        {
            matchData.MatchMetadata.Competition = CleanText(matchData.MatchMetadata.Competition, NameMaxLength)!;
            if (matchData.MatchMetadata.Competition == null)
            {
                errors.Add("Missing competition name.");
            }

            if (MatchDataUtils.GetMatchDate(matchData) == null)
            {
                errors.Add($"Unparseable match date '{matchData.MatchMetadata.Date}'.");
            }

            if (!TryParseScore(matchData.MatchMetadata.FinalScore, out _, out _))
            {
                errors.Add($"Invalid final score '{matchData.MatchMetadata.FinalScore}'.");
            }
        }

        SanitizeTeam(matchData.HomeTeam, "home", errors);
        SanitizeTeam(matchData.AwayTeam, "away", errors);

        if (matchData.HomeTeam?.Name != null
            && string.Equals(matchData.HomeTeam.Name, matchData.AwayTeam?.Name, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Home and away teams are the same.");
        }

        return errors;
    }

    private static void SanitizeTeam(TeamData? team, string side, List<string> errors)
    {
        if (team == null)
        {
            errors.Add($"Missing {side}_team.");
            return;
        }

        team.Name = CleanText(team.Name, NameMaxLength)!;
        if (team.Name == null)
        {
            errors.Add($"Missing {side} team name.");
        }

        if (team.Players == null)
        {
            return;
        }

        foreach (var player in team.Players)
        {
            player.Name = CleanText(player.Name, NameMaxLength)!;
            player.Position = CleanText(player.Position, PositionMaxLength)!;
        }

        team.Players = team.Players
            .Where(p => p.Name != null)
            .DistinctBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

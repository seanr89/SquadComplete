using System;
using System.Globalization;
using Squad.Function.Models.AI;

namespace Squad.Function.Utils;

public static class MatchDataUtils
{
    /// <summary>
    /// Parses the AI-supplied match date as UTC. Returns null if missing/unparseable.
    /// </summary>
    public static DateTime? GetMatchDate(MatchDetails? matchData)
    {
        if (!DateTime.TryParse(matchData?.MatchMetadata?.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return null;
        }
        // Npgsql only accepts Kind=Utc for 'timestamp with time zone' columns.
        return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
    }
}

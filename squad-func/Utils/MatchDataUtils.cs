using System;
using Squad.Function.Models.AI;

namespace Squad.Function.Utils;

public static class MatchDataUtils
{
    public static DateTime? GetMatchDate(MatchDetails? matchData)
    {
        DateTime? matchDate = null;
        if (DateTime.TryParse(matchData?.MatchMetadata?.Date, out var parsedDate))
        {
            matchDate = parsedDate;
        }
        // update matchdate to resolve issue: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
        matchDate = DateTime.SpecifyKind(matchDate ?? DateTime.MinValue, DateTimeKind.Utc);
        return matchDate;
    }
}

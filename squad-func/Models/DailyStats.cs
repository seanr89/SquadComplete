

public class DailyStats
{
    public DateTime Date { get; set; } = DateTime.Now.AddDays(-1);
    public int TotalTeams { get; set; } = 0;
    public int ActiveTeams { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int TotalPlayers { get; set; } = 0;
    public int TotalUserSquads { get; set; } = 0;
    public int TotalGameRecords { get; set; } = 0;
    public int FixturesMissingTeams { get; set; } = 0;
    public int FixturesMissingScores { get; set; } = 0;
    public int AIFixtureCount { get; set; } = 0;
    public int FixturesMissingDates { get; set; } = 0;


    public override string ToString()
    {
        return $@"Date: {Date.ToLongDateString()}
            Total Teams: {TotalTeams}
            Active Teams: {ActiveTeams}
            Total Matches: {TotalMatches}
            Total Players: {TotalPlayers}
            Total User Squads: {TotalUserSquads}
            Total Game Records: {TotalGameRecords}
            Fixtures Missing Teams: {FixturesMissingTeams}
            Fixtures Missing Scores: {FixturesMissingScores}
            Fixtures Missing Dates: {FixturesMissingDates}
            AI Fixtures: {AIFixtureCount}";
    }
}
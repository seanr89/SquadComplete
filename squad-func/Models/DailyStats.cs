

public class DailyStats
{
    public DateTime Date { get; set; } = DateTime.Now.AddDays(-1);
    public int TotalTeams { get; set; } = 0;
    public int ActiveTeams { get; set; } = 0;
    public int TotalMatches { get; set; } = 0;
    public int TotalPlayers { get; set; } = 0;
    public int TotalUserSquads { get; set; } = 0;
    public int TotalGameRecords { get; set; } = 0;


    public override string ToString()
    {
        return $@"Date: {Date.ToShortDateString()}
            Total Teams: {TotalTeams}
            Active Teams: {ActiveTeams}
            Total Matches: {TotalMatches}
            Total Players: {TotalPlayers}
            Total User Squads: {TotalUserSquads}
            Total Game Records: {TotalGameRecords}";
    }
}
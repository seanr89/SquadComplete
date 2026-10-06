namespace squad_func.Models;

// Response contracts for GET /api/game-records/date/{date}. These mirror squad-api's
// GameRecordDto / FormationResponse so the client can swap base URLs without changes.

public class GameRecordDto
{
    public int Id { get; set; }
    public DateTime GameDate { get; set; }
    public FormationResponse Formation { get; set; } = new(0, "Unknown", 0, 0, 0);
    public List<GameRecordTeamDto> Teams { get; set; } = new();
}

public class GameRecordTeamDto
{
    public int? FixtureId { get; set; }
    public int? TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public string? TeamLogo { get; set; }
    public string Formation { get; set; } = string.Empty;
    public List<GameRecordPlayerDto> Players { get; set; } = new();
}

public class GameRecordPlayerDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string? PlayerPhoto { get; set; }
    public GameRecordPlayerStatisticDto? Statistics { get; set; }
}

public class GameRecordPlayerStatisticDto
{
    public string? Position { get; set; }
    public decimal? Rating { get; set; }
}

public record FormationResponse(int Id, string Name, int Defence, int Midfield, int Attack)
{
    public static FormationResponse From(Formation f) => new(f.Id, f.Name, f.Defence, f.Midfield, f.Attack);
}

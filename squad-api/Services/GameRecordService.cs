using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.DTOs;

namespace squad_api.Services;

public class GameRecordService
{
    private readonly SquadContext _db;

    public GameRecordService(SquadContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets all game records
    /// </summary>
    /// <returns>List of GameRecordDto objects</returns>
    public async Task<List<GameRecordDto>> GetAllGameRecordsAsync()
    {
        var records = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
            .Include(gr => gr.Formation)
            .ToListAsync();

        var fixtureIds = records.SelectMany(r => r.Tags).Select(t => t.FixtureId).Distinct().ToList();
        var teamIds = records.SelectMany(r => r.Tags).Select(t => t.TeamId).Distinct().ToList();

        var statistics = await _db.PlayerFixtureStatistics
            .Include(s => s.Player)
            .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value))
            .ToListAsync();

        return records.Select(r => MapToDto(r, statistics)).ToList();
    }

    /// <summary>
    /// Gets a game record by ID
    /// </summary>
    /// <param name="id">ID of the game record</param>
    /// <returns>GameRecordDto object</returns>
    public async Task<GameRecordDto?> GetGameRecordByIdAsync(int id)
    {
        var record = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
            .Include(gr => gr.Formation)
            .FirstOrDefaultAsync(gr => gr.Id == id);

        if (record == null) return null;

        var fixtureIds = record.Tags.Select(t => t.FixtureId).Distinct().ToList();
        var teamIds = record.Tags.Select(t => t.TeamId).Distinct().ToList();

        var statistics = await _db.PlayerFixtureStatistics
            .Include(s => s.Player)
            .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value))
            .ToListAsync();

        return MapToDto(record, statistics);
    }

    /// <summary>
    /// Gets a game record by date
    /// </summary>
    /// <param name="date">Date of the game record</param>
    /// <returns>GameRecordDto object</returns>
    public async Task<GameRecordDto?> GetGameRecordByDateAsync(DateTime date)
    {
        var record = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
            .Include(gr => gr.Formation)
            .FirstOrDefaultAsync(gr => gr.GameDate.ToUniversalTime().Date == date.ToUniversalTime().Date);

        if (record == null) return null;

        var fixtureIds = record.Tags.Select(t => t.FixtureId).Distinct().ToList();
        var teamIds = record.Tags.Select(t => t.TeamId).Distinct().ToList();

        var statistics = await _db.PlayerFixtureStatistics
            .Include(s => s.Player)
            .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value) && s.IsSubstitute == false)
            .ToListAsync();

        return MapToDto(record, statistics);
    }

    /// <summary>
    /// Creates a new game record
    /// </summary>
    /// <param name="record">GameRecord object to create</param>
    /// <returns>GameRecordDto object</returns>
    public async Task<GameRecordDto> CreateGameRecordAsync(GameRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        _db.GameRecords.Add(record);
        await _db.SaveChangesAsync();

        // Fetch it back with tags to map to DTO
        return await GetGameRecordByIdAsync(record.Id) ?? MapToDto(record, new List<PlayerFixtureStatistic>());
    }

    /// <summary>
    /// Maps a GameRecord and PlayerFixtureStatistic objects to a GameRecordDto object
    /// </summary>
    /// <param name="record">GameRecord object to map</param>
    /// <param name="statistics">List of PlayerFixtureStatistic objects to map</param>
    /// <returns>GameRecordDto object</returns>
    public GameRecordDto MapToDto(GameRecord record, List<PlayerFixtureStatistic> statistics)
    {
        return new GameRecordDto
        {
            Id = record.Id,
            GameDate = record.GameDate,
            Formation = record.Formation ?? new Formation { Name = "Unknown", Id = 0, Defence = 4, Midfield = 4, Attack = 2 },
            Teams = record.Tags.Select(t =>
            {
                var players = statistics
                    .Where(s => s.FixtureId == t.FixtureId && s.TeamId == t.TeamId)
                    .Select(s => new GameRecordPlayerDto
                    {
                        PlayerId = s.PlayerId,
                        PlayerName = s.Player?.Name ?? string.Empty,
                        PlayerPhoto = s.Player?.Photo,
                        Statistics = new GameRecordPlayerStatisticDto
                        {
                            // Minutes = s.Minutes,
                            // Number = s.Number,
                            Position = MapPosition(s.Position),
                            Rating = s.Rating,
                            // IsCaptain = s.IsCaptain,
                            // IsSubstitute = s.IsSubstitute
                        }
                    }).ToList();

                return new GameRecordTeamDto
                {
                    FixtureId = t.FixtureId,
                    TeamId = t.TeamId,
                    TeamName = t.Team?.Name ?? string.Empty,
                    TeamLogo = t.Team?.Logo,
                    Formation = CalculateFormation(players),
                    Players = players
                };
            }).ToList()
        };
    }

    /// <summary>
    /// Maps a position string to a position enum
    /// </summary>
    /// <param name="position">Position string to map</param>
    /// <returns>Position enum</returns>
    private string MapPosition(string? position)
    {
        if (string.IsNullOrEmpty(position)) return "UNK";

        var pos = position.ToUpper();
        if (pos.Contains("GOALKEEPER") || pos == "G" || pos == "GK" || pos == "@P5") return "GK";
        if (pos.Contains("DEFENDER") || pos == "D" || pos == "DEF" || pos == "LB" || pos == "RB" || pos == "CB") return "DEF";
        if (pos.Contains("MIDFIELDER") || pos == "M" || pos == "MID" || pos == "CM" || pos == "DM" || pos == "AM") return "MID";
        if (pos.Contains("FORWARD") || pos == "F" || pos == "FWD" || pos == "ST" || pos == "LW" || pos == "RW") return "FWD";

        return pos;
    }

    /// <summary>
    /// Calculates the expected team formation
    /// </summary>
    /// <param name="players">List of GameRecordPlayerDto objects</param>
    /// <returns>Formation string (e.g., 4-4-2)</returns>
    private string CalculateFormation(List<GameRecordPlayerDto> players)
    {
        var defenderCount = players.Count(p => p.Statistics?.Position == "DEF");
        var midfielderCount = players.Count(p => p.Statistics?.Position == "MID");
        var attackerCount = players.Count(p => p.Statistics?.Position == "FWD");

        return $"{defenderCount}-{midfielderCount}-{attackerCount}";
    }
}

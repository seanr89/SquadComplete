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

    public async Task<List<GameRecordDto>> GetAllGameRecordsAsync()
    {
        var records = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
            .ToListAsync();

        var fixtureIds = records.SelectMany(r => r.Tags).Select(t => t.FixtureId).Distinct().ToList();
        var teamIds = records.SelectMany(r => r.Tags).Select(t => t.TeamId).Distinct().ToList();

        var statistics = await _db.PlayerFixtureStatistics
            .Include(s => s.Player)
            .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value))
            .ToListAsync();

        return records.Select(r => MapToDto(r, statistics)).ToList();
    }

    public async Task<GameRecordDto?> GetGameRecordByIdAsync(int id)
    {
        var record = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
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

    public async Task<GameRecordDto?> GetGameRecordByDateAsync(DateTime date)
    {
        var record = await _db.GameRecords
            .Include(gr => gr.Tags)
                .ThenInclude(t => t.Team)
            .FirstOrDefaultAsync(gr => gr.GameDate.ToUniversalTime().Date == date.ToUniversalTime().Date);

        if (record == null) return null;

        var fixtureIds = record.Tags.Select(t => t.FixtureId).Distinct().ToList();
        var teamIds = record.Tags.Select(t => t.TeamId).Distinct().ToList();

        var statistics = await _db.PlayerFixtureStatistics
            .Include(s => s.Player)
            .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value))
            .ToListAsync();

        return MapToDto(record, statistics);
    }
    
    public async Task<GameRecordDto> CreateGameRecordAsync(GameRecord record)
    {
        record.CreatedAt = DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        _db.GameRecords.Add(record);
        await _db.SaveChangesAsync();
        
        // Fetch it back with tags to map to DTO
        return await GetGameRecordByIdAsync(record.Id) ?? MapToDto(record, new List<PlayerFixtureStatistic>());
    }

    public GameRecordDto MapToDto(GameRecord record, List<PlayerFixtureStatistic> statistics)
    {
        return new GameRecordDto
        {
            Id = record.Id,
            GameDate = record.GameDate,
            //CreatedAt = record.CreatedAt,
            //UpdatedAt = record.UpdatedAt,
            Teams = record.Tags.Select(t => new GameRecordTeamDto
            {
                //FixtureId = t.FixtureId,
                //TeamId = t.TeamId,
                TeamName = t.Team?.Name ?? string.Empty,
                TeamLogo = t.Team?.Logo,
                Players = statistics
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
                            Position = s.Position,
                            Rating = s.Rating,
                            // IsCaptain = s.IsCaptain,
                            // IsSubstitute = s.IsSubstitute
                        }
                    }).ToList()
            }).ToList()
        };
    }
}

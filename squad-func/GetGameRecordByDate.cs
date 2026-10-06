using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace Squad.Function;

public class GetGameRecordByDate(ILogger<GetGameRecordByDate> logger, SquadContext context)
{
    private readonly ILogger<GetGameRecordByDate> _logger = logger;
    private readonly SquadContext _context = context;

    // Mirrors squad-api's GET /api/game-records/date/{date} so the client can swap base URLs.
    [Function("GetGameRecordByDate")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "game-records/date/{date}")] HttpRequest req,
        string date)
    {
        if (!DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            return new BadRequestObjectResult("Date must be in yyyy-MM-dd format.");
        }

        var dayStart = DateTime.SpecifyKind(parsed.Date, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        try
        {
            var record = await _context.GameRecords
                .AsNoTracking()
                .Include(gr => gr.Tags)
                    .ThenInclude(t => t.Team)
                .Include(gr => gr.Formation)
                .FirstOrDefaultAsync(gr => gr.GameDate >= dayStart && gr.GameDate < dayEnd);

            if (record is null)
            {
                return new NotFoundResult();
            }

            var fixtureIds = record.Tags.Select(t => t.FixtureId).Distinct().ToList();
            var teamIds = record.Tags.Select(t => t.TeamId).Distinct().ToList();

            var statistics = await _context.PlayerFixtureStatistics
                .AsNoTracking()
                .Include(s => s.Player)
                .Where(s => fixtureIds.Contains(s.FixtureId)
                    && s.TeamId != null
                    && teamIds.Contains(s.TeamId.Value)
                    && !s.IsSubstitute)
                .ToListAsync();

            return new OkObjectResult(MapToDto(record, statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load game record for {Date}.", date);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }

    private static GameRecordDto MapToDto(GameRecord record, List<PlayerFixtureStatistic> statistics)
    {
        return new GameRecordDto
        {
            Id = record.Id,
            GameDate = record.GameDate,
            Formation = record.Formation is null
                ? new FormationResponse(0, "Unknown", 4, 4, 2)
                : FormationResponse.From(record.Formation),
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
                            Position = MapPosition(s.Position),
                            Rating = s.Rating
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

    // Known positions become GK/DEF/MID/FWD; empty is UNK; unrecognised values pass through upper-cased.
    private static string MapPosition(string? position)
    {
        var group = PositionGroupExtensions.Parse(position);

        if (group == PositionGroup.Unknown && !string.IsNullOrEmpty(position))
        {
            return position.ToUpperInvariant();
        }

        return group.ToCode();
    }

    private static string CalculateFormation(List<GameRecordPlayerDto> players)
    {
        var defenders = players.Count(p => p.Statistics?.Position == PositionGroup.Defender.ToCode());
        var midfielders = players.Count(p => p.Statistics?.Position == PositionGroup.Midfielder.ToCode());
        var attackers = players.Count(p => p.Statistics?.Position == PositionGroup.Forward.ToCode());

        return $"{defenders}-{midfielders}-{attackers}";
    }
}

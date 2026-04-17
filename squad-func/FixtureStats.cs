using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using squad_func.Services;

namespace Squad.Function;

public class FixtureStats(ILoggerFactory loggerFactory, SquadContext context,
IApiService apiService, DatabaseService databaseService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<FixtureStats>();
    private readonly SquadContext _context = context;
    private readonly IApiService _apiService = apiService;
    // add dbservice
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// Azure Function trigger that runs on a schedule to fetch player statistics for fixtures that don't have them yet.
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("FixtureStats")]
    public async Task Run([TimerTrigger("0 0 12-16 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        try
        {
            // get fixtures that don't have player stats currently and take 8
            var fixturesWithoutStats = await _context.Fixtures
                .Where(f => !_context.PlayerFixtureStatistics.Any(pfs => pfs.FixtureId == f.Id))
                .OrderBy(f => f.Id)
                .Take(8)
                .ToListAsync();

            if (fixturesWithoutStats.Count == 0)
            {
                _logger.LogWarning("No fixtures without player statistics found.");
                return;
            }

            foreach (var fixture in fixturesWithoutStats)
            {
                if (fixture.HomeTeamId != null)
                {
                    var homeTeamStats = await _apiService.GetPlayerStatsAsync(fixture.Id, fixture.HomeTeamId.Value);
                    await ProcessTeamStatsAsync(fixture.Id, fixture.HomeTeamId.Value, homeTeamStats);
                }
                if (fixture.AwayTeamId != null)
                {
                    var awayTeamStats = await _apiService.GetPlayerStatsAsync(fixture.Id, fixture.AwayTeamId.Value);
                    await ProcessTeamStatsAsync(fixture.Id, fixture.AwayTeamId.Value, awayTeamStats);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during fixture stats retrieval.");
        }

        // get updated count
        var fixturesWithoutStatsUpdated = await _context.Fixtures
            .Where(f => !_context.PlayerFixtureStatistics.Any(pfs => pfs.FixtureId == f.Id))
            .CountAsync();

        _logger.LogInformation("Found {count} fixtures without player statistics.", fixturesWithoutStatsUpdated);

        // if (myTimer.ScheduleStatus is not null)
        // {
        //     _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        // }
    }

    /// <summary>
    /// Processes and persists team, player, and statistical data from the API response to the database.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture the stats belong to.</param>
    /// <param name="teamId">The ID of the team being processed.</param>
    /// <param name="teamStats">The list of player statistics from the API.</param>
    private async Task ProcessTeamStatsAsync(int fixtureId, int teamId, List<PlayerStatsResponse>? teamStats)
    {
        if (teamStats == null) return;

        foreach (var response in teamStats)
        {
            if (response.Team != null)
            {
                await _databaseService.UpsertTeamAsync(response.Team.Id, response.Team.Name ?? "", response.Team.Logo, response.Team.Update);
            }

            if (response.Players != null)
            {
                foreach (var playerData in response.Players)
                {
                    if (playerData.Player != null)
                    {
                        await _databaseService.UpsertPlayerAsync(playerData.Player.Id, playerData.Player.Name ?? "", playerData.Player.Photo);

                        var stat = playerData.Statistics?.FirstOrDefault();
                        if (stat != null)
                        {
                            if (stat.Games?.Substitute == true)
                            {
                                continue;
                            }

                            decimal? rating = null;
                            if (decimal.TryParse(stat.Games?.Rating, out var parsedRating))
                            {
                                rating = parsedRating > 0 ? parsedRating : 0.0m;
                            }

                            await _databaseService.UpsertPlayerStatsAsync(
                                fixtureId,
                                teamId,
                                playerData.Player.Id,
                                stat.Games?.Minutes,
                                stat.Games?.Number,
                                stat.Games?.Position,
                                rating,
                                stat.Games?.Captain ?? false,
                                stat.Games?.Substitute ?? false
                            );
                        }
                    }
                }
            }
        }
    }
}
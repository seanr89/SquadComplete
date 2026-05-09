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

public class TeamRefresh(ILoggerFactory loggerFactory, SquadContext context,
IApiService apiService, DatabaseService databaseService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<TeamRefresh>();
    private readonly SquadContext _context = context;
    private readonly IApiService _apiService = apiService;
    // add dbservice
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// Function to refresh player and team info
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("TeamRefresh")]
    public async Task Run([TimerTrigger("0 0 4-11 * * *")] TimerInfo myTimer)
    {
        //_logger.LogInformation("TeamRefresh started");

        var incompleteFixtures = await _context.Fixtures
            .Where(f => f.HomeTeamId == null || f.AwayTeamId == null
                || f.HomeTeamName == null || f.AwayTeamName == null)
            .OrderBy(f => f.CreatedAt)
            .Take(8)
            .ToListAsync();

        _logger.LogInformation("Found {Count} fixtures with missing team information.", incompleteFixtures.Count);

        foreach (var fixture in incompleteFixtures)
        {
            try
            {
                var fixtureData = await _apiService.GetFixtureDataAsync(fixture.Id);
                Thread.Sleep(2500);
                if (fixtureData?.Teams != null)
                {
                    if (fixtureData.Teams.Home != null)
                    {
                        var homeTeam = fixtureData.Teams.Home;
                        fixture.HomeTeamId = homeTeam.Id;
                        fixture.HomeTeamName = homeTeam.Name;
                        //fixture.FixtureDate = fixtureData?.Fixture?.Date;

                        var exists = await _context.Teams.AnyAsync(t => t.Id == homeTeam.Id);
                        if (!exists)
                        {
                            var apiTeam = await _apiService.GetTeamDataAsync(homeTeam.Id);
                            Thread.Sleep(2500);
                            if (apiTeam != null)
                            {
                                await _databaseService.UpsertTeamAsync(apiTeam.Id, apiTeam.Name ?? homeTeam.Name ?? "Unknown", apiTeam.Logo ?? homeTeam.Logo, null);
                            }
                            else
                            {
                                await _databaseService.UpsertTeamAsync(homeTeam.Id, homeTeam.Name ?? "Unknown", homeTeam.Logo, null);
                            }
                        }

                        await UpdateFixtureDetailsAsync(fixture.Id, fixtureData);
                    }

                    if (fixtureData.Teams.Away != null)
                    {
                        var awayTeam = fixtureData.Teams.Away;
                        fixture.AwayTeamId = awayTeam.Id;
                        fixture.AwayTeamName = awayTeam.Name;


                        var exists = await _context.Teams.AnyAsync(t => t.Id == awayTeam.Id);
                        if (!exists)
                        {
                            var apiTeam = await _apiService.GetTeamDataAsync(awayTeam.Id);
                            Thread.Sleep(2500);
                            if (apiTeam != null)
                            {
                                await _databaseService.UpsertTeamAsync(apiTeam.Id, apiTeam.Name ?? awayTeam.Name ?? "Unknown", apiTeam.Logo ?? awayTeam.Logo, null);
                            }
                            else
                            {
                                await _databaseService.UpsertTeamAsync(awayTeam.Id, awayTeam.Name ?? "Unknown", awayTeam.Logo, null);
                            }
                        }

                        await UpdateFixtureDetailsAsync(fixture.Id, fixtureData);
                    }

                    _logger.LogInformation("Updated team information for fixture {FixtureId}.", fixture.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fixture {FixtureId} for team info refresh.", fixture.Id);
            }
        }

        await _context.SaveChangesAsync();
        //_logger.LogInformation("TeamRefresh completed");
    }
    private async Task UpdateFixtureDetailsAsync(int fixtureId, FixtureApiResponse fixtureData)
    {
        var dbFixture = await _context.Fixtures.FindAsync(fixtureId);
        if (dbFixture != null)
        {
            dbFixture.HomeGoalCount = fixtureData.Goals?.Home;
            dbFixture.AwayGoalCount = fixtureData.Goals?.Away;
            dbFixture.HomeTeamId = fixtureData.Teams?.Home?.Id;
            dbFixture.AwayTeamId = fixtureData.Teams?.Away?.Id;
            dbFixture.HomeTeamName = fixtureData.Teams?.Home?.Name;
            dbFixture.AwayTeamName = fixtureData.Teams?.Away?.Name;
            dbFixture.UpdatedAt = DateTime.UtcNow;
            dbFixture.FixtureDate = fixtureData?.Fixture?.Date != null
                ? DateTime.SpecifyKind(fixtureData.Fixture.Date.Value, DateTimeKind.Utc)
                : null;

            await _context.SaveChangesAsync();
        }
    }
}
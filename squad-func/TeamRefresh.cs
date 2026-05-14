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
    public async Task Run([TimerTrigger("0 0 5-9 * * *")] TimerInfo myTimer)
    {
        // re-working logic flow here to check via fixture date is not null
        var incompleteFixtures = await _context.Fixtures
            .Where(f => f.FixtureDate == null && f.ApiId != null)
            .OrderBy(f => f.CreatedAt)
            .Take(4)
            .ToListAsync();

        _logger.LogInformation("Found {Count} fixtures with missing team information.", incompleteFixtures.Count);

        foreach (var fixture in incompleteFixtures)
        {
            try
            {
                var fixtureData = await _apiService.GetFixtureDataAsync(fixture.ApiId ?? 0);
                Thread.Sleep(2500);
                if (fixtureData?.Teams != null)
                {
                    await UpdateFixtureDetailsAsync(fixture.Id, fixtureData);
                    _logger.LogInformation("Updated team information for fixture {FixtureId}.", fixture.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fixture {FixtureId} for team info refresh.", fixture.Id);
            }
        }

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the fixture with the team information.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture.</param>
    /// <param name="fixtureData">The fixture data from the API.</param>
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
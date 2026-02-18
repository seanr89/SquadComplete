using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Squad.Function;

public class SquadSelector
{
    private readonly ILogger _logger;
    private readonly SquadContext _context;

    public SquadSelector(ILoggerFactory loggerFactory, SquadContext context)
    {
        _logger = loggerFactory.CreateLogger<SquadSelector>();
        _context = context;
    }

    [Function("SquadSelector")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        try
        {
            var fixtures = await _context.Fixtures
                .Include(f => f.League)
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {count} fixtures from the database.", fixtures.Count);

            // Randomize the list of fixtures
            var random = new Random();
            var shuffledFixtures = fixtures.OrderBy(x => random.Next()).ToList();

            // Identify the first 11 unique team IDs
            var uniqueTeamIds = new HashSet<int>();
            foreach (var fixture in shuffledFixtures)
            {
                if (fixture.HomeTeamId.HasValue)
                {
                    uniqueTeamIds.Add(fixture.HomeTeamId.Value);
                }

                if (uniqueTeamIds.Count >= 11) break;

                if (fixture.AwayTeamId.HasValue)
                {
                    uniqueTeamIds.Add(fixture.AwayTeamId.Value);
                }

                if (uniqueTeamIds.Count >= 11) break;
            }

            _logger.LogInformation("Identified {count} unique team IDs: {ids}", 
                uniqueTeamIds.Count, string.Join(", ", uniqueTeamIds));

            if (uniqueTeamIds.Count < 11)
            {
                _logger.LogWarning("Only found {count} unique team IDs, expected 11.", uniqueTeamIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during fixture selection.");
        }
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
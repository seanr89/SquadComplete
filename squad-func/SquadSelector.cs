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

            // Retrieve all formations and select a random one
            var formations = await _context.Formations.ToListAsync();
            Formation? selectedFormation = null;
            if (formations.Any())
            {
                selectedFormation = formations[random.Next(formations.Count)];
                _logger.LogInformation("Selected random Formation with ID: {id} ({name})", selectedFormation.Id, selectedFormation.Name);
            }
            else
            {
                _logger.LogWarning("No formations found in the database.");
            }

            // Create a new game record for today
            var gameRecord = new GameRecord
            {
                GameDate = DateTime.UtcNow.Date,
                FormationId = selectedFormation?.Id
            };
            
            _context.GameRecords.Add(gameRecord);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Created new GameRecord with ID: {id}", gameRecord.Id);

            // Identify the first 11 unique team IDs and create tags
            var uniqueTeamIds = new HashSet<int>();
            var tagsToAdd = new List<GameRecordTag>();

            foreach (var fixture in shuffledFixtures)
            {
                // add player fixture check to ensure that player stats are present first
                // var playerFixture = await _context.PlayerFixtureStatistics
                //     .FirstOrDefaultAsync(pf => pf.FixtureId == fixture.Id);

                // get player fixture count for home and away teams
                var homePlayerFixtureCount = await _context.PlayerFixtureStatistics
                    .CountAsync(pf => pf.FixtureId == fixture.Id && pf.TeamId == fixture.HomeTeamId);

                var awayPlayerFixtureCount = await _context.PlayerFixtureStatistics
                    .CountAsync(pf => pf.FixtureId == fixture.Id && pf.TeamId == fixture.AwayTeamId);

                if (homePlayerFixtureCount == 0 && awayPlayerFixtureCount == 0)
                {
                    _logger.LogInformation("Player fixture not found for fixture {fixtureId}", fixture.Id);
                    continue;
                }

                if (fixture.HomeTeamId.HasValue && homePlayerFixtureCount >= 11 )
                {
                    tagsToAdd.Add(new GameRecordTag
                    {
                        GameRecordId = gameRecord.Id,
                        FixtureId = fixture.Id,
                        TeamId = fixture.HomeTeamId.Value
                    });
                }

                if (uniqueTeamIds.Count >= 11) break;

                if (fixture.AwayTeamId.HasValue && awayPlayerFixtureCount >= 11)
                {
                    tagsToAdd.Add(new GameRecordTag
                    {
                        GameRecordId = gameRecord.Id,
                        FixtureId = fixture.Id,
                        TeamId = fixture.AwayTeamId.Value
                    });
                }

                if (uniqueTeamIds.Count >= 11) break;
            }

            if (tagsToAdd.Any())
            {
                _context.GameRecordTags.AddRange(tagsToAdd);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {count} tags to GameRecord {id}. Teams: {ids}",
                    tagsToAdd.Count, gameRecord.Id, string.Join(", ", uniqueTeamIds));
            }

            if (uniqueTeamIds.Count < 11)
            {
                _logger.LogWarning("Only found {count} unique team IDs, expected 11.", uniqueTeamIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during game creation.");
            _logger.LogError(ex.Message);
            _logger.LogError(ex.InnerException?.Message);
        }
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
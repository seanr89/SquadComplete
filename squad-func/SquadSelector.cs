using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Squad.Function;

public class SquadSelector(ILoggerFactory loggerFactory, SquadContext context)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SquadSelector>();
    private readonly SquadContext _context = context;

    [Function("SquadSelector")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo myTimer)
    {
        //_logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        try
        {
            int totalFixtures = await _context.Fixtures.CountAsync();
            // In future, this will need to limited/paginated for performance reasons
            var fixtures = await _context.Fixtures
                .Include(f => f.League)
                .ToListAsync();

            // Randomize the list of fixtures
            var random = new Random();
            var shuffledFixtures = fixtures.OrderBy(x => random.Next()).ToList();

            // Retrieve all formations and select a random one
            var formations = await _context.Formations.ToListAsync();
            Formation? selectedFormation = null;
            if (formations.Count != 0)
            {
                selectedFormation = formations[random.Next(formations.Count)];
            }

            // Create a new game record for today
            var gameRecord = new GameRecord
            {
                GameDate = DateTime.UtcNow.Date,
                Formation = selectedFormation
            };

            _context.GameRecords.Add(gameRecord);
            await _context.SaveChangesAsync();

            // Identify the first 11 unique team IDs and create tags
            var uniqueTeamIds = new HashSet<int>();
            var tagsToAdd = new List<GameRecordTag>();

            foreach (var fixture in shuffledFixtures)
            {
                // get player fixture count for home and away teams to ensure we have enough players
                var homePlayerFixtureCount = await _context.PlayerFixtureStatistics
                    .CountAsync(pf => pf.FixtureId == fixture.Id && pf.TeamId == fixture.HomeTeamId);

                var awayPlayerFixtureCount = await _context.PlayerFixtureStatistics
                    .CountAsync(pf => pf.FixtureId == fixture.Id && pf.TeamId == fixture.AwayTeamId);

                // we need at least 11 players for a team to be selected if they are to be included in the game
                if (homePlayerFixtureCount == 0 && awayPlayerFixtureCount == 0)
                {
                    continue;
                }

                // Convert this to its own function with a try add the unique teams
                TryAddTeamTag(fixture.HomeTeamId, homePlayerFixtureCount, fixture.Id, gameRecord.Id, uniqueTeamIds, tagsToAdd);

                if (uniqueTeamIds.Count >= 11) break;

                TryAddTeamTag(fixture.AwayTeamId, awayPlayerFixtureCount, fixture.Id, gameRecord.Id, uniqueTeamIds, tagsToAdd);

                if (uniqueTeamIds.Count >= 11) break;
            }

            if (tagsToAdd.Count != 0)
            {
                _context.GameRecordTags.AddRange(tagsToAdd);
                await _context.SaveChangesAsync();
                // _logger.LogInformation("Added {count} tags to GameRecord {id}. Teams: {ids}",
                //     tagsToAdd.Count, gameRecord.Id, string.Join(", ", uniqueTeamIds));
            }

            if (uniqueTeamIds.Count < 11)
            {
                //_logger.LogWarning("Only found {count} unique team IDs, expected 11.", uniqueTeamIds.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during game creation.");
            _logger.LogError(ex.Message);
            _logger.LogError(ex.InnerException?.Message);
        }
    }

    /// <summary>
    /// Tries to add a team tag to the list of tags to add if the team has at least 11 players.
    /// </summary>
    /// <param name="teamId">The ID of the team to add.</param>
    /// <param name="playerCount">The number of players for the team.</param>
    /// <param name="fixtureId">The ID of the fixture the team is part of.</param>
    /// <param name="gameRecordId">The ID of the game record the team is part of.</param>
    /// <param name="uniqueTeamIds">The set of unique team IDs.</param>
    /// <param name="tagsToAdd">The list of tags to add.</param> 
    private static void TryAddTeamTag(int? teamId, int playerCount, int fixtureId, int gameRecordId, HashSet<int> uniqueTeamIds, List<GameRecordTag> tagsToAdd)
    {
        if (teamId.HasValue && playerCount >= 11)
        {
            if (uniqueTeamIds.Add(teamId.Value))
            {
                tagsToAdd.Add(new GameRecordTag
                {
                    GameRecordId = gameRecordId,
                    FixtureId = fixtureId,
                    TeamId = teamId.Value
                });
            }
        }
    }
}
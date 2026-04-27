using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace squad_func.Services;

public class AgentMappingService : IAgentMappingService
{
    private readonly SquadContext _context;
    private readonly ILogger<AgentMappingService> _logger;

    public AgentMappingService(SquadContext context, ILogger<AgentMappingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes an AgentFixture and saves it to the database.
    /// </summary>
    /// <param name="agentFixture">The AgentFixture to process.</param>
    /// <returns></returns>
    public async Task ProcessAgentFixtureAsync(AgentFixture agentFixture)
    {
        if (agentFixture == null || agentFixture.Matches == null || agentFixture.Matches.Count == 0)
        {
            _logger.LogWarning("AgentFixture is null or contains no matches.");
            return;
        }

        // Determine next IDs for entities to manually assign them
        int nextLeagueId = (await _context.Leagues.MaxAsync(l => (int?)l.Id) ?? 0) + 1;
        int nextTeamId = (await _context.Teams.MaxAsync(t => (int?)t.Id) ?? 0) + 1;
        int nextPlayerId = (await _context.Players.MaxAsync(p => (int?)p.Id) ?? 0) + 1;
        int nextFixtureId = (await _context.Fixtures.MaxAsync(f => (int?)f.Id) ?? 0) + 1;

        // Local caches to prevent duplicate creation in the same batch
        var leagueCache = await _context.Leagues.ToDictionaryAsync(l => l.Name, l => l);
        var teamCache = await _context.Teams.ToDictionaryAsync(t => t.Name, t => t);
        var playerCache = await _context.Players.ToDictionaryAsync(p => p.Name, p => p);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Process League
            League? league = null;
            if (!string.IsNullOrWhiteSpace(agentFixture.League))
            {
                if (!leagueCache.TryGetValue(agentFixture.League, out league))
                {
                    league = new League { Id = nextLeagueId++, Name = agentFixture.League };
                    _context.Leagues.Add(league);
                    leagueCache[league.Name] = league;
                }
            }

            foreach (var match in agentFixture.Matches)
            {
                if (match.Fixture == null) continue;

                var homeTeamName = match.Fixture.HomeTeam;
                var awayTeamName = match.Fixture.AwayTeam;

                Team? homeTeam = null;
                Team? awayTeam = null;

                // Process Home Team
                if (!string.IsNullOrWhiteSpace(homeTeamName))
                {
                    if (!teamCache.TryGetValue(homeTeamName, out homeTeam))
                    {
                        homeTeam = new Team { Id = nextTeamId++, Name = homeTeamName, Active = true };
                        _context.Teams.Add(homeTeam);
                        teamCache[homeTeam.Name] = homeTeam;
                    }
                }

                // Process Away Team
                if (!string.IsNullOrWhiteSpace(awayTeamName))
                {
                    if (!teamCache.TryGetValue(awayTeamName, out awayTeam))
                    {
                        awayTeam = new Team { Id = nextTeamId++, Name = awayTeamName, Active = true };
                        _context.Teams.Add(awayTeam);
                        teamCache[awayTeam.Name] = awayTeam;
                    }
                }

                // Create Fixture
                var fixture = new Fixture
                {
                    Id = nextFixtureId++,
                    LeagueId = league?.Id,
                    HomeTeamId = homeTeam?.Id,
                    HomeTeamName = homeTeam?.Name,
                    AwayTeamId = awayTeam?.Id,
                    AwayTeamName = awayTeam?.Name,
                    HomeGoalCount = match.Score?.HomeScore,
                    AwayGoalCount = match.Score?.AwayScore
                };
                _context.Fixtures.Add(fixture);

                // Process Lineups
                if (match.Lineups != null)
                {
                    await ProcessLineupAsync(match.Lineups.HomeStartingXi, homeTeam?.Id, fixture, false, playerCache, ref nextPlayerId);
                    await ProcessLineupAsync(match.Lineups.HomeSubstitutes, homeTeam?.Id, fixture, true, playerCache, ref nextPlayerId);
                    await ProcessLineupAsync(match.Lineups.AwayStartingXi, awayTeam?.Id, fixture, false, playerCache, ref nextPlayerId);
                    await ProcessLineupAsync(match.Lineups.AwaySubstitutes, awayTeam?.Id, fixture, true, playerCache, ref nextPlayerId);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Successfully mapped and saved AgentFixture data.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error occurred while mapping AgentFixture to database entities.");
            throw;
        }
    }

    /// <summary>
    /// Processes a list of player names and adds them to the database.
    /// </summary>
    /// <param name="playerNames">The list of player names to process.</param>
    /// <param name="teamId">The ID of the team the players belong to.</param>
    /// <param name="fixture">The fixture the players belong to.</param>
    /// <param name="isSubstitute">Whether the players are substitutes.</param>
    /// <param name="playerCache"></param>
    /// <param name="nextPlayerId"></param>
    /// <returns></returns>
    private Task ProcessLineupAsync(List<string>? playerNames, int? teamId, Fixture fixture, bool isSubstitute, Dictionary<string, Player> playerCache, ref int nextPlayerId)
    {
        if (playerNames == null || !playerNames.Any()) return Task.CompletedTask;

        foreach (var playerName in playerNames)
        {
            if (string.IsNullOrWhiteSpace(playerName)) continue;

            if (!playerCache.TryGetValue(playerName, out var player))
            {
                player = new Player { Id = nextPlayerId++, Name = playerName };
                _context.Players.Add(player);
                playerCache[player.Name] = player;
            }

            var stat = new PlayerFixtureStatistic
            {
                FixtureId = fixture.Id,
                Fixture = fixture,
                TeamId = teamId,
                PlayerId = player.Id,
                Player = player,
                IsSubstitute = isSubstitute,
                IsCaptain = false // Agent data doesn't provide captain info currently
            };

            _context.PlayerFixtureStatistics.Add(stat);
        }

        return Task.CompletedTask;
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using squad_func.Services;
using System.Text.Json;
using Squad.Function.Models.AI;
using Squad.Function.Models.API;
using Squad.Function.Utils;

namespace Squad.Function;

public class GenerateFixtureFromAIMatchData(ILoggerFactory loggerFactory, SquadContext context,
    StorageService storageService, IApiService apiService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GenerateFixtureFromAIMatchData>();
    private readonly SquadContext _context = context;
    private readonly StorageService _storageService = storageService;
    private readonly IApiService _apiService = apiService;

    /// <summary>
    /// Function to generate a fixture for a single historical match using Gemini AI 
    /// This will attempt to ingest a file from AzureStorage, grab and process the match
    /// Use the FootballData API to get the player and team info to fill out the fixture if possible
    /// else just build a dummy match record
    /// Schedule - 20:30 every day (19:30 UTC) - 
    /// this is to allow the function to run after the Gemini function that generates the match data file runs at 19:00 UTC
    /// 0 30 19-21 * * *
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("GenerateFixtureFromAIMatchData")]
    public async Task Run([TimerTrigger("0 0,30 15-21 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("GenerateFixtureFromAIMatchData triggered at: {CurrentUtcDateTime}", DateTime.UtcNow);
        try
        {
            /* Steps here
            1. Check for any files and grab 1 if available
            2. Identifiy competition and TryGet/Create League
            3. Identify teams and TryGet/Create teams
            4. Create a fixture record
            5. Create all player records and add to match then create all the playerfixturemappings
            6. Delete the blob
            7. Check if there are more blobs to process
            */

            var blobs = await _storageService.GetBlobs("ai-team-single");
            var blob = blobs.FirstOrDefault() ?? throw new Exception("No blob found in ai-team-single container");
            var data = await _storageService.ReadFromStorage(blob, "ai-team-single");


            var matchData = JsonSerializer.Deserialize<MatchDetails>(data);

            League? dbLeague = await GetOrCreateLeague(matchData);
            if (dbLeague == null)
            {
                _logger.LogError("Could not find or create league for match");
                await _storageService.MoveBlob(blob, "ai-team-single", "archive-league-error");
                return;
            }

            _logger.LogInformation("Got league now get or create teams for fixture!");

            string? homeTeamName = matchData?.HomeTeam?.Name;
            var dbHomeTeam = _context.Teams.FirstOrDefault(t => t.Name == homeTeamName);
            string? awayTeamName = matchData?.AwayTeam?.Name;
            var dbAwayTeam = _context.Teams.FirstOrDefault(t => t.Name == awayTeamName);

            dbHomeTeam ??= await FindAndCreateTeamIfNotExists(matchData, homeTeamName, dbHomeTeam);
            dbAwayTeam ??= await FindAndCreateTeamIfNotExists(matchData, awayTeamName, dbAwayTeam);
            DateTime? matchDate = MatchDataUtils.GetMatchDate(matchData);
            if (matchDate == null)
            {
                _logger.LogError("Could not parse match date");
                return;
            }
            _logger.LogInformation("Got teams now check if fixture already exists for {HomeTeam} vs {AwayTeam} on {MatchDate}", homeTeamName, awayTeamName, matchDate);

            // lets try and find the fixture in the database that matches team id etc...
            var dbFixture = _context.Fixtures.FirstOrDefault(
                f => f.LeagueId == dbLeague.Id && f.HomeTeamId == dbHomeTeam.Id && f.AwayTeamId == dbAwayTeam.Id
                && f.FixtureDate == matchDate);
            if (dbFixture != null)
            {
                _logger.LogWarning("Fixture already exists for {HomeTeam} vs {AwayTeam} on {MatchDate}, skipping creation", homeTeamName, awayTeamName, matchDate);
                return;
            }

            var homePlayers = matchData?.HomeTeam?.Players;
            var awayPlayers = matchData?.AwayTeam?.Players;

            // API based query data from API Calls
            List<PlayerAPIModel> homePlayersFound = [];
            List<PlayerAPIModel> awayPlayersFound = [];

            // Database based players  
            List<Player> dbHomePlayers = [];
            List<Player> dbAwayPlayers = [];

            // Players for mapping to a player fixture mapping db
            List<MappedPlayer> mappedHomePlayers = [];
            List<MappedPlayer> mappedAwayPlayers = [];

            if (homePlayers != null && dbHomeTeam.Active == true)
            {
                await AddPlayersToMappedPlayerList(homePlayers, homePlayersFound, dbHomePlayers, mappedHomePlayers); // End Player For Loop
            }

            if (awayPlayers != null && dbAwayTeam.Active == true)
            {
                await AddPlayersToMappedPlayerList(awayPlayers, awayPlayersFound, dbAwayPlayers, mappedAwayPlayers);
            }

            _logger.LogInformation("Players found for fixture {FixtureId}: {HomePlayers} {AwayPlayers}", dbFixture?.Id, dbHomePlayers.Count, dbAwayPlayers.Count);

            // check that there are at least 11 players from each team else the data set was wrong
            if (dbHomePlayers.Count < 11 && dbHomeTeam.Active == true)
            {
                // Not enough players for the fixture - skip
                _logger.LogWarning("Not enough home team players for fixture: {HomeTeam} vs {AwayTeam}", dbHomeTeam.Name, dbAwayTeam.Name);
                return;
            }
            if (dbAwayPlayers.Count < 11 && dbAwayTeam.Active == true)
            {
                // Not enough players for the fixture - skip
                _logger.LogWarning("Not enough for away team for fixture: {HomeTeam} vs {AwayTeam}", dbHomeTeam.Name, dbAwayTeam.Name);
                return;
            }

            // Parse score
            int homeGoalCount = 0;
            int awayGoalCount = 0;
            var scoreParts = matchData?.MatchMetadata?.FinalScore?.Split("-");
            if (scoreParts != null && scoreParts.Length >= 2)
            {
                _ = int.TryParse(scoreParts[0], out homeGoalCount);
                _ = int.TryParse(scoreParts[1], out awayGoalCount);
            }

            _logger.LogInformation("Got Scores now create fixture!");
            Fixture newFixture = CreateNewFixtureAndSave(matchData, dbLeague, dbHomeTeam,
                dbAwayTeam, matchDate, homeGoalCount, awayGoalCount);

            // Now I need to include any and all players from the home and away teams into the player fixture stats
            if (dbHomePlayers.Count > 0)
            {
                AddPlayerFixtureStats(dbHomeTeam, dbHomePlayers, mappedHomePlayers, newFixture);
            }
            if (dbAwayPlayers.Count > 0)
            {
                AddPlayerFixtureStats(dbAwayTeam, dbAwayPlayers, mappedAwayPlayers, newFixture);
            }

            _context.SaveChanges();
            //Now lets move the blob to archive
            await _storageService.MoveBlob(blob, "ai-team-single", "archive");
        }
        catch (Exception ex)
        {
            _logger.LogError("GenerateFixtureFromAIMatchData error msg : {ex.Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// TODO: Refactor this to be more testable and break down into smaller methods - this is doing a lot right now
    /// </summary>
    /// <param name="matchData"></param>
    /// <param name="dbLeague"></param>
    /// <param name="dbHomeTeam"></param>
    /// <param name="dbAwayTeam"></param>
    /// <param name="matchDate"></param>
    /// <param name="homeGoalCount"></param>
    /// <param name="awayGoalCount"></param>
    /// <returns></returns>
    private Fixture CreateNewFixtureAndSave(MatchDetails? matchData, League dbLeague,
        Team dbHomeTeam, Team dbAwayTeam,
        DateTime? matchDate, int homeGoalCount, int awayGoalCount)
    {
        var newFixture = new Fixture
        {
            LeagueId = dbLeague.Id,
            HomeTeamId = dbHomeTeam.Id,
            AwayTeamId = dbAwayTeam.Id,
            HomeTeamName = matchData?.HomeTeam?.Name,
            AwayTeamName = matchData?.AwayTeam?.Name,
            HomeGoalCount = homeGoalCount,
            AwayGoalCount = awayGoalCount,
            FixtureDate = matchDate,
            FixtureSource = "AI"
        };
        _context.Fixtures.Add(newFixture);
        _context.SaveChanges();
        return newFixture;
    }

    /// <summary>
    /// Gets or creates a league in the database
    /// </summary>
    /// <param name="matchData">The match data</param>
    /// <returns>The league</returns>
    private async Task<League> GetOrCreateLeague(MatchDetails? matchData)
    {
        string? competitionName = matchData?.MatchMetadata?.Competition;
        var dbLeague = _context.Leagues.FirstOrDefault(l => l.Name == competitionName);
        dbLeague ??= await HandleNewLeagueRequest(matchData, competitionName, dbLeague);
        return dbLeague;
    }

    /// <summary>
    /// Adds the player fixture stats to the database
    /// </summary>
    /// <param name="dbHomeTeam">The home team</param>
    /// <param name="dbHomePlayers">The list of home players</param>
    /// <param name="mappedHomePlayers">The list of mapped home players</param>
    /// <param name="newFixture">The new fixture</param>
    private void AddPlayerFixtureStats(Team dbHomeTeam, List<Player> dbHomePlayers, List<MappedPlayer> mappedHomePlayers, Fixture newFixture)
    {
        foreach (var player in dbHomePlayers)
        {
            var playerMapped = mappedHomePlayers.FirstOrDefault(p => p.dbPlayer.Id == player.Id);
            var newPlayerFixtureStat = new PlayerFixtureStatistic
            {
                PlayerId = player.Id,
                FixtureId = newFixture.Id,
                TeamId = dbHomeTeam.Id,
                Position = playerMapped?.filePlayerData?.Position ?? "N/A",
                Rating = (decimal?)playerMapped?.filePlayerData?.Rating ?? 0.0m
            };
            _context.PlayerFixtureStatistics.Add(newPlayerFixtureStat);
        }
    }

    /// <summary>
    /// Adds the players to the mapped player list
    /// </summary>
    /// <param name="homePlayers">The list of players to add</param>
    /// <param name="homePlayersFound">The list of players found</param>
    /// <param name="dbHomePlayers">The list of players in the database</param>
    /// <param name="mappedHomePlayers">The list of mapped players</param>
    private async Task AddPlayersToMappedPlayerList(List<Models.AI.PlayerData> homePlayers,
        List<PlayerAPIModel> homePlayersFound,
        List<Player> dbHomePlayers,
        List<MappedPlayer> mappedHomePlayers)
    {
        foreach (var player in homePlayers)
        {
            // check to see if the player is already in the database
            var dbPlayer = _context.Players.FirstOrDefault(p => p.Name == player.Name);
            if (dbPlayer != null)
            {
                dbHomePlayers.Add(dbPlayer);
                mappedHomePlayers.Add(new MappedPlayer(dbPlayer, null, player));
            }
            else
            {
                var playerResponse = await _apiService.GetPlayerByNameAsync(player.Name);
                Thread.Sleep(2500);
                try
                {
                    var foundPlayer = JsonSerializer.Deserialize<PlayerAPIModel>(playerResponse);
                    if (foundPlayer != null && foundPlayer.Results > 0)
                    {
                        var matchedResponseItem = foundPlayer?.Response?.FirstOrDefault(r =>
                        r.Player != null && (
                            string.Equals(r.Player.Name, player.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals($"{r.Player.Firstname} {r.Player.Lastname}", player.Name, StringComparison.OrdinalIgnoreCase)
                        ));

                        if (matchedResponseItem != null && matchedResponseItem.Player != null)
                        {
                            var matchedPlayerModel = foundPlayer! with { Response = new List<PlayerResponseItem> { matchedResponseItem } };
                            homePlayersFound.Add(matchedPlayerModel);
                            // now need to add to the db
                            var newPlayer = new Player
                            {
                                ApiId = matchedResponseItem.Player.Id,
                                Name = matchedResponseItem.Player.Firstname + " " + matchedResponseItem.Player.Lastname ?? "N/A",
                                Photo = matchedResponseItem.Player.Photo ?? "N/A"
                            };
                            _context.Players.Add(newPlayer);
                            _context.SaveChanges();
                            dbHomePlayers.Add(newPlayer);
                            mappedHomePlayers.Add(new MappedPlayer(newPlayer, matchedPlayerModel, player));

                        }
                        else
                        {
                            // so if no find, then we just make one up for now
                            var bespokePlayer = new Player
                            {
                                Name = player.Name
                            };
                            _context.Players.Add(bespokePlayer);
                            _context.SaveChanges();
                            dbHomePlayers.Add(bespokePlayer);
                            mappedHomePlayers.Add(new MappedPlayer(bespokePlayer, null, player));
                        }
                    }
                    else
                    {
                        var bespokePlayer = new Player
                        {
                            Name = player.Name
                        };
                        _context.Players.Add(bespokePlayer);
                        _context.SaveChanges();
                        dbHomePlayers.Add(bespokePlayer);
                        mappedHomePlayers.Add(new MappedPlayer(bespokePlayer, null, player));
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError("GenerateFixtureFromAIMatchData error msg : {ex.Message}", ex.Message);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Finds and creates a team if it doesn't exist
    /// </summary>
    /// <param name="matchData">The match data</param>
    /// <param name="homeTeamName">The name of the home team</param>
    /// <param name="dbHomeTeam">The database home team</param>
    /// <returns></returns>
    private async Task<Team> FindAndCreateTeamIfNotExists(MatchDetails? matchData, string? homeTeamName, Team? dbHomeTeam)
    {
        _logger.LogInformation("Getting team by name for: {TeamName}", homeTeamName);
        var homeTeamResponse = await _apiService.GetTeamByNameAsync(homeTeamName);
        Thread.Sleep(2500);
        var homeTeam = JsonSerializer.Deserialize<TeamAPIModel>(homeTeamResponse);

        if (homeTeam != null && homeTeam?.Response?.Count > 0)
        {
            //_logger.LogInformation($"Found home team: {homeTeam?.Response?.First()?.Team?.Name}");
            // lets make a new team record
            var newHomeTeam = new Team
            {
                ApiId = homeTeam?.Response?.First()?.Team?.Id ?? 0,
                Name = homeTeam?.Response?.First()?.Team?.Name ?? "N/A",
                Logo = homeTeam?.Response?.First()?.Team?.Logo ?? "N/A"
            };
            _context.Teams.Add(newHomeTeam);
            dbHomeTeam = newHomeTeam;
        }
        else
        {
            _logger.LogInformation("Could not find team: {TeamName}", homeTeamName);
            var newHomeTeam = new Team
            {
                Name = homeTeamName ?? "N/A"
            };
            _context.Teams.Add(newHomeTeam);
            dbHomeTeam = newHomeTeam;
        }
        await _context.SaveChangesAsync();

        return dbHomeTeam;
    }

    /// <summary>
    /// Handles new league requests
    /// </summary>
    /// <param name="matchData">The match data</param>
    /// <param name="competitionName">The name of the competition</param>
    /// <param name="dbLeague">The database league</param>
    /// <returns></returns>
    private async Task<League> HandleNewLeagueRequest(MatchDetails? matchData, string? competitionName, League dbLeague)
    {
        _logger.LogInformation("Could not find league: {CompetitionName}", competitionName);
        // step2. I want to get the league id from the competition name
        var leagueResponse = await _apiService.GetLeagueByNameAsync(matchData?.MatchMetadata?.Competition);
        try
        {
            var league = JsonSerializer.Deserialize<LeagueAPIModel>(leagueResponse);
            if (league != null && league?.Response?.Count > 0)
            {
                _logger.LogInformation("Found league: {LeagueName}", league?.Response?.First()?.League?.Name);
                // lets make a new league record
                var newLeague = new League
                {
                    ApiId = league?.Response?.First()?.League?.Id ?? 0,
                    Name = league?.Response?.First()?.League?.Name ?? "N/A",
                    Logo = league?.Response?.First()?.League?.Logo ?? "N/A"
                };
                _context.Leagues.Add(newLeague);
                await _context.SaveChangesAsync();
                dbLeague = newLeague;
            }
            else
            {
                _logger.LogInformation("Could not find league: {CompetitionName}", competitionName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching league: {CompetitionName}", competitionName);
        }

        return dbLeague;
    }
}
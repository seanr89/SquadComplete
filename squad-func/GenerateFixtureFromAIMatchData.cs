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
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("GenerateFixtureFromAIMatchData")]
    public async Task Run([TimerTrigger("0 30 20 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("GenerateFixtureFromAIMatchData triggered at: {CurrentUtcDateTime}", DateTime.UtcNow);
        try
        {
            var blobs = await _storageService.GetBlobs("ai-team-single");
            if (blobs.Count <= 0)
            {
                _logger.LogWarning("No blobs found in ai-team-single storage. This is expected if no ai team single data has been recorded.");
                return;
            }
            var blob = blobs.First();

            var data = await _storageService.ReadFromStorage(blob, "ai-team-single");
            if (string.IsNullOrEmpty(data)) return;

            var matchData = JsonSerializer.Deserialize<MatchDetails>(data);

            string? competitionName = matchData?.MatchMetadata?.Competition;
            var dbLeague = _context.Leagues.FirstOrDefault(l => l.Name == competitionName);
            dbLeague ??= await HandleNewLeagueRequest(matchData, competitionName, dbLeague);

            string? homeTeamName = matchData?.HomeTeam?.Name;
            var dbHomeTeam = _context.Teams.FirstOrDefault(t => t.Name == homeTeamName);
            string? awayTeamName = matchData?.AwayTeam?.Name;
            var dbAwayTeam = _context.Teams.FirstOrDefault(t => t.Name == awayTeamName);

            dbHomeTeam ??= await FindAndCreateTeamIfNotExists(matchData, homeTeamName, dbHomeTeam);

            dbAwayTeam ??= await FindAndCreateTeamIfNotExists(matchData, awayTeamName, dbAwayTeam);

            DateTime? matchDate = null;
            if (DateTime.TryParse(matchData?.MatchMetadata?.Date, out var parsedDate))
            {
                matchDate = parsedDate;
            }
            // update matchdate to resolve issue: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone', only UTC is supported. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
            matchDate = DateTime.SpecifyKind(matchDate ?? DateTime.MinValue, DateTimeKind.Utc);

            // lets try and find the fixture in the database
            var dbFixture = _context.Fixtures.FirstOrDefault(
                f => f.LeagueId == dbLeague.Id && f.HomeTeamId == dbHomeTeam.Id && f.AwayTeamId == dbAwayTeam.Id
                && f.FixtureDate == matchDate);
            if (dbFixture != null)
            {
                Console.WriteLine($"Found fixture: {dbFixture.Id} - we may need to scrape this match, but we have the record for now");
                //continue;
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

            if (dbHomePlayers.Count < 11 && dbAwayPlayers.Count < 11)
            {
                // Not enough players for the fixture - skip
                Console.WriteLine($"Not enough players for fixture: {dbHomeTeam.Name} vs {dbAwayTeam.Name}");
                return;
            }
            // Parse score
            int homeGoalCount = 0;
            int awayGoalCount = 0;
            var scoreParts = matchData?.MatchMetadata?.FinalScore?.Split("-");
            if (scoreParts != null && scoreParts.Length >= 2)
            {
                int.TryParse(scoreParts[0], out homeGoalCount);
                int.TryParse(scoreParts[1], out awayGoalCount);
            }

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
            _logger.LogError(ex, "Error generating fixture from MatchData");
            _logger.LogError("AI error is : {ex.Message}", ex.Message);
            _logger.LogError("AI error is : {ex.Source}", ex.Source);
            _logger.LogError("AI error is : {ex.StackTrace}", ex.StackTrace);
            throw;
        }
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
                Position = playerMapped?.apiPlayer?.Response?.First()?.Player?.Position ?? "N/A",
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
                Console.WriteLine($"Found DB Player: {dbPlayer.Name} {dbPlayer.Id}");
                dbHomePlayers.Add(dbPlayer);
                mappedHomePlayers.Add(new MappedPlayer(dbPlayer, null, player));
                //continue;
            }
            else
            {
                Console.WriteLine($"Could not find DB Player: {player.Name} - make call to API");
                var playerResponse = await _apiService.GetPlayerByNameAsync(player.Name);
                try
                {
                    Thread.Sleep(2000);
                    var foundPlayer = JsonSerializer.Deserialize<PlayerAPIModel>(playerResponse);
                    var matchedResponseItem = foundPlayer?.Response?.FirstOrDefault(r =>
                        r.Player != null && (
                            string.Equals(r.Player.Name, player.Name, StringComparison.OrdinalIgnoreCase) ||
                            string.Equals($"{r.Player.Firstname} {r.Player.Lastname}", player.Name, StringComparison.OrdinalIgnoreCase)
                        ));

                    if (matchedResponseItem != null && matchedResponseItem.Player != null)
                    {
                        var matchedPlayerModel = foundPlayer! with { Response = new List<PlayerResponseItem> { matchedResponseItem } };
                        Console.WriteLine($"Found player: {matchedResponseItem.Player.Name} {matchedResponseItem.Player.Id}");
                        homePlayersFound.Add(matchedPlayerModel);
                        // now need to add to the db
                        var newPlayer = new Player
                        {
                            ApiId = matchedResponseItem.Player.Id,
                            Name = matchedResponseItem.Player.Name ?? "N/A",
                            Photo = matchedResponseItem.Player.Photo ?? "N/A"
                        };
                        _context.Players.Add(newPlayer);
                        _context.SaveChanges();
                        dbHomePlayers.Add(newPlayer);
                        mappedHomePlayers.Add(new MappedPlayer(newPlayer, matchedPlayerModel, player));

                    }
                    else
                    {
                        Console.WriteLine($"Could not find player: {player.Name} - WARNING - need to handle");
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error finding player: {player.Name} {ex.Message}");
                    throw;
                }
                Thread.Sleep(2500);
            }
        }
    }

    private async Task<Team> FindAndCreateTeamIfNotExists(MatchDetails? matchData, string? homeTeamName, Team dbHomeTeam)
    {
        var homeTeamResponse = await _apiService.GetTeamByNameAsync(matchData?.HomeTeam?.Name);
        var homeTeam = JsonSerializer.Deserialize<TeamAPIModel>(homeTeamResponse);

        if (homeTeam != null)
        {
            Console.WriteLine($"Found home team: {homeTeam?.Response?.First()?.Team?.Name}");
            // lets make a new team record
            var newHomeTeam = new Team
            {
                Id = homeTeam?.Response?.First()?.Team?.Id ?? 0,
                Name = homeTeam?.Response?.First()?.Team?.Name ?? "N/A",
                Logo = homeTeam?.Response?.First()?.Team?.Logo ?? "N/A"
            };
            _context.Teams.Add(newHomeTeam);
            _context.SaveChanges();
            dbHomeTeam = newHomeTeam;
        }
        else
        {
            _logger.LogInformation("Could not find home team: {TeamName}", homeTeamName);
            //continue;
        }

        return dbHomeTeam;
    }

    private async Task<League> HandleNewLeagueRequest(MatchDetails? matchData, string? competitionName, League dbLeague)
    {
        _logger.LogInformation("Could not find league: {CompetitionName}", competitionName);
        // step2. I want to get the league id from the competition name
        var leagueResponse = await _apiService.GetLeagueByNameAsync(matchData?.MatchMetadata?.Competition);
        var league = JsonSerializer.Deserialize<LeagueAPIModel>(leagueResponse);
        if (league != null && league.Response.Count > 0)
        {
            _logger.LogInformation("Found league: {LeagueName}", league?.Response?.First()?.League?.Name);
            // lets make a new league record
            var newLeague = new League
            {
                Id = league?.Response?.First()?.League?.Id ?? 0,
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

        return dbLeague;
    }
}
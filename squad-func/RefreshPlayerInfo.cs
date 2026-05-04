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

public class RefreshPlayerInfo(ILoggerFactory loggerFactory, SquadContext context,
IApiService apiService, DatabaseService databaseService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RefreshPlayerInfo>();
    private readonly SquadContext _context = context;
    private readonly IApiService _apiService = apiService;
    // add dbservice
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// Function to refresh player and team info
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("RefreshPlayerInfo")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("RefreshPlayerInfo started");

        var playersWithoutPhoto = await _context.Players
            .Where(p => string.IsNullOrEmpty(p.Photo))
            .OrderBy(p => p.CreatedAt)
            .Take(20) // to avoid hitting API rate limits too hard
            .ToListAsync();

        _logger.LogInformation("Found {Count} players without a photo.", playersWithoutPhoto.Count);

        foreach (var player in playersWithoutPhoto)
        {
            try
            {
                var lastName = player.Name.Split(' ').Last();
                var playerProfile = await _apiService.GetPlayerProfileAsync(lastName);
                Thread.Sleep(2500);

                if (playerProfile != null && !string.IsNullOrEmpty(playerProfile.Photo))
                {
                    player.Photo = playerProfile.Photo;
                    await _databaseService.UpsertPlayerAsync(player.Id, player.Name, playerProfile.Photo);
                    _logger.LogInformation("Updated photo for player {PlayerId} ({PlayerName}).", player.Id, player.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing player {PlayerId} for photo refresh.", player.Id);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("RefreshPlayerInfo completed");
    }
}
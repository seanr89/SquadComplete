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

namespace Squad.Function;

public class SingleMatchHistoricalSearch(ILoggerFactory loggerFactory, SquadContext context,
GeminiService geminiService, StorageService storageService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SingleMatchHistoricalSearch>();
    private readonly SquadContext _context = context;
    private readonly GeminiService _geminiService = geminiService;
    private readonly StorageService _storageService = storageService;

    /// <summary>
    /// Function to refresh player and team info
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("SingleMatchHistoricalSearch")]
    public async Task Run([TimerTrigger("0 30 6 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("SingleMatchHistoricalSearch started");

        // step 1. lets run and see if there is a historical record/file to search
        if (await _storageService.IsContainerEmpty("ai-team"))
        {
            _logger.LogInformation("Team container is empty, nothing to do");
            return;
        }

        // Step 2. read down file from storage
        var blobs = await _storageService.GetBlobs("ai-team");
        var blob = blobs.First();
        var blobData = await _storageService.ReadFromStorage(blob, "ai-team");

        try
        {
            var seasonMatchData = JsonSerializer.Deserialize<SeasonData>(blobData);
            // Proceed with logic
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing blob data");
            throw;
        }

        // Step 3. read next record for a historical match

        // Step 4. Run a gemini search via prompt, passing it the match info (this is)

        // Step 5. Validate response format etc...

        // Step 6 build object model from stored data for Player, Team data etc...

        // Step 7. Use DbContext to update each of the record's teams, and players to include their respective photos etc... 

        // Step 8. Remove from the historical file and save back to storage for next time

    }
}
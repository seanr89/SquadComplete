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
    public async Task Run([TimerTrigger("0 15-45 11-16 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("SingleMatchHistoricalSearch started");

        // step 1. lets run and see if there is a historical record/file to search
        if (await _storageService.IsContainerEmpty("ai-team") == true)
        {
            _logger.LogInformation("Team container is empty, nothing to do");
            return;
        }

        // Step 2. read down file from storage
        var blobs = await _storageService.GetBlobs("ai-team");
        var blob = blobs.First();
        var blobData = await _storageService.ReadFromStorage(blob, "ai-team");

        //Get the Name of the team Searched
        var teamName = blob.Split("_")[0];
        var seasonDate = blob.Split("_")[1];
        seasonDate = seasonDate.Replace("-", "/");

        SeasonData seasonMatchData;
        try
        {
            seasonMatchData = JsonSerializer.Deserialize<SeasonData>(blobData);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing blob data");
            throw;
        }

        // Step 3. read next record for a historical match
        var seasonMatch = seasonMatchData?.Fixtures.FirstOrDefault();

        // Step 4. Run a gemini search via prompt, passing it the match info (this is)
        string matchDate = seasonMatch?.Date;
        var geminiResponse = await _geminiService.GetSingleMatchHistoryAsync(teamName, seasonDate, matchDate);

        if (string.IsNullOrEmpty(geminiResponse))
        {
            _logger.LogError("HistoricalAi failed for {Team} {Season}", teamName, seasonDate);
            return;
        }
        // Step 5. Validate response format etc... using the matchmetatdata
        MatchDetails matchMetaData;
        try
        {
            matchMetaData = JsonSerializer.Deserialize<MatchDetails>(geminiResponse);

            //Todo - lets save the record to storage
            var filename = $"{teamName}_{matchDate}.json";
            await _storageService.UploadToStorage(geminiResponse, filename, "ai-team-single");

            // now we need to update the historical search
            seasonMatchData.Fixtures.Remove(seasonMatch);
            await _storageService.UploadToStorage(JsonSerializer.Serialize(seasonMatchData), blob, "ai-team");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing blob data");
            throw;
        }

    }
}
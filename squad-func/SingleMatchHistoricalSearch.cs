using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using squad_func.Services;
using System.Text.Json;
using Squad.Function.Models.AI;

namespace Squad.Function;

public class SingleMatchHistoricalSearch(ILoggerFactory loggerFactory,
GeminiService geminiService, StorageService storageService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<SingleMatchHistoricalSearch>();
    private readonly GeminiService _geminiService = geminiService;
    private readonly StorageService _storageService = storageService;

    /// <summary>
    /// Function to refresh player and team info for a single historical match using Gemini AI 
    /// This is run for the previous days fixtures in a given season
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("SingleMatchHistoricalSearch")]
    public async Task Run([TimerTrigger("0 15,45 15-16 * * *")] TimerInfo myTimer)
    {
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

        SeasonData? seasonMatchData;
        try
        {
            seasonMatchData = JsonSerializer.Deserialize<SeasonData>(blobData);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error deserializing blob data with error {Error}", ex.Message);
            throw;
        }

        // Step 3. read next record for a historical match
        var seasonMatch = seasonMatchData?.Fixtures.FirstOrDefault();
        if (seasonMatch == null)
        {
            _logger.LogInformation("No more historical matches for {Team} {Season}", teamName, seasonDate);
            //we can remove the blob at this point from storage as it has been processed
            await _storageService.MoveBlob(blob, "ai-team", "history-completed");
            return;
        }

        // Step 4. Run a gemini search via prompt, passing it the match info (this is)
        string matchDate = seasonMatch?.Date;
        var geminiResponse = await _geminiService.GetSingleMatchHistoryAsync(teamName, seasonDate, matchDate);

        if (string.IsNullOrEmpty(geminiResponse))
        {
            _logger.LogError("HistoricalAi failed for {Team} {Season}", teamName, seasonDate);
            return;
        }
        // Step 5. Validate response format etc... using the matchmetatdata
        MatchDetails? matchMetaData;
        try
        {
            using JsonDocument doc = JsonDocument.Parse(geminiResponse);
            var root = doc.RootElement;
            var textResponse = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (textResponse != null)
            {
                textResponse = textResponse.Trim();
                if (textResponse.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
                {
                    textResponse = textResponse.Substring(7);
                }
                else if (textResponse.StartsWith("```"))
                {
                    textResponse = textResponse.Substring(3);
                }

                if (textResponse.EndsWith("```"))
                {
                    textResponse = textResponse.Substring(0, textResponse.Length - 3);
                }

                textResponse = textResponse.Trim();
            }

            matchMetaData = JsonSerializer.Deserialize<MatchDetails>(textResponse ?? string.Empty);
            _logger.LogInformation("Match details for {Team} {Season} {Match}", teamName, seasonDate, matchDate);

            //Lets the save the data
            var filename = $"{teamName}_{matchDate}.json";
            await _storageService.UploadToStorage(JsonSerializer.Serialize(matchMetaData), filename, "ai-team-single");

            // now we need to update the historical search
            seasonMatchData?.Fixtures?.Remove(seasonMatch);
            await _storageService.UploadToStorage(JsonSerializer.Serialize(seasonMatchData), blob, "ai-team");

            // now we move the blob to history-completed
            if (seasonMatchData?.Fixtures?.Count == 0)
            {
                await _storageService.MoveBlob(blob, "ai-team", "history-completed");
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error processing SingleMatchLogic {Error}", ex.Message);
            throw;
        }

    }
}
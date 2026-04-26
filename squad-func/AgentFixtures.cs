using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;
using System.Text.Json;

namespace Squad.Function;

public class AgentFixtures(ILoggerFactory loggerFactory,
GeminiService geminiService, StorageService storageService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<AgentFixtures>();
    private readonly GeminiService _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    [Function("AgentFixtures")]
    public async Task Run([TimerTrigger("0 0 20 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("AgentFixtures started");
        // set current date -1 day
        DateTime currentDate = DateTime.Now.AddDays(-1);
        string formattedDate = currentDate.ToString("yyyy-MM-dd");
        try
        {
            // create prompt message
            string userPrompt = $"find me English premier league matches for date {formattedDate} in json format";

            // call api service
            string? response = await _geminiService.GenerateContentAsync(userPrompt);
            if (response == null)
            {
                _logger.LogWarning("No response from Gemini Query");
                return;
            }

            // convert response to json
            string? jsonResponse = ConvertResponseToJson(response);

            if (!string.IsNullOrEmpty(jsonResponse))
            {
                // save json to blob storage
                await _storageService.UploadToStorage(jsonResponse, $"agent-fixtures-{formattedDate}.json", "agent-fixtures");
            }
            else
            {
                _logger.LogWarning("No valid json response for Gemini Query was returned");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during fixture stats retrieval.");
        }
    }

    /// <summary>
    /// Converts the response from Gemini to JSON format
    /// </summary>
    /// <param name="aiText">The response from Gemini</param>
    /// <returns>The JSON response</returns>
    private static string? ConvertResponseToJson(string aiText)
    {
        // --- Extract and Save JSON ---
        try
        {
            string jsonContent = string.Empty;
            //
            var jsonRegex = new System.Text.RegularExpressions.Regex(@"```json\s*([\s\S]*?)\s*```", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var match = jsonRegex.Match(aiText);

            if (match.Success)
            {
                jsonContent = match.Groups[1].Value;
            }
            else if (aiText.Trim().StartsWith("{") && aiText.Trim().EndsWith("}"))
            {
                jsonContent = aiText.Trim();
            }
            return jsonContent;
        }
        catch (JsonException)
        {
            Console.WriteLine("Note: A JSON-like block was found but it contains invalid JSON.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Note: Could not extract JSON: {ex.Message}");
        }

        return string.Empty;
    }
}
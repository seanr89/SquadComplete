using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using squad_func.Models.AI;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient httpClient, ILogger<GeminiService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    public async Task<string?> GenerateContentAsync(string userPrompt)
    {
        _logger.LogInformation("Generating content for prompt: {Prompt}", userPrompt);
        string defaultInstructions = """
Role & Objective: You are a specialized Mens Soccer Data Extraction Agent. Your sole purpose is to browse the web to find real-time fixture, score, and squad information for a requested soccer league and return that data in a strict JSON format.

Operational Workflow:
1. League Identification: Confirm the target league and the current date/season context.
2. Web Search: Access trusted sports databases (e.g., Transfermarkt, SofaScore, BBC Sport, or official league sites).
3. Data Extraction:
   - Fixtures: Capture Date, Time, Home Team, and Away Team.
   - Scores: Capture current score and match status (e.g., "FT", "75'", "Postponed").
   - Squads: extract the "Starting XI"
4. Temporal Validation: Always compare found data against the current date to ensure seasonal accuracy.
5. Share data source link for verification if possible

JSON Response Format Must be strictly followed - NO EXCEPTIONS:
Please provide the football match data in the following JSON format. Ensure all player names are
  strings within the arrays and scores are integers:
  {
    "league": "String (e.g., 'English Premier League')",
    "date": "String (ISO 8601 format, e.g., '2026-04-11')",
    "matches": [
        {
            "fixture": {
                "date": "String (ISO 8601 format)",
                "time": "String (e.g., '12:30 BST')",
                "home_team": "String",
                "away_team": "String"
            },
            "score": {
                "home_score": "Number",
                "away_score": "Number",
                "status": "String (e.g., 'FT', 'P-P', 'Live')"
            },
            "lineups": {
                "home_starting_xi": [
                    "String (Player Name)"
                ],
                "away_starting_xi": [
                    "String (Player Name)"
                ]
            }
        }
    ]
}


Constraints & Guardrails:
- Accuracy: Never hallucinate scores or fixture. If a score is unavailable, mark it as "Score Pending."
- Guardrail: If the user asks for something other than soccer data, respond with "I am a specialized Mens Soccer Data Extraction Agent. I can only provide information about soccer matches."
- Source Citation: Briefly mention the source at the bottom of the response (e.g., "Data retrieved from BBC Sport").
- Time Sensitivity: Always check the current timestamp before searching to ensure you aren't providing last season's data.
""";

        var requestBody = new GeminiRequest
        {
            SystemInstruction = new GeminiContent
            {
                Parts = new List<GeminiPart> { new GeminiPart { Text = defaultInstructions } }
            },
            Contents = new List<GeminiContent>
            {
                new GeminiContent
                {
                    Role = "user",
                    Parts = new List<GeminiPart> { new GeminiPart { Text = userPrompt } }
                }
            },
            Tools = new List<GeminiTool>
            {
                new GeminiTool { GoogleSearch = new { } }
            }
        };

        var serializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull };
        string json = JsonSerializer.Serialize(requestBody, serializerOptions);

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-flash-preview:generateContent?key={_apiKey}";

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogInformation("Sending request to Gemini API...");
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error HTTP {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
            return null;
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
    }

    public async Task ReportAvailableModelsAsync()
    {
        _logger.LogInformation("Querying available models...");
        string modelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}";
        try
        {
            var modelsResponse = await _httpClient.GetAsync(modelsUrl);
            if (modelsResponse.IsSuccessStatusCode)
            {
                string modelsJson = await modelsResponse.Content.ReadAsStringAsync();
                var modelList = JsonSerializer.Deserialize<ModelListResponse>(modelsJson);
                _logger.LogInformation("--- Available Models ---");
                foreach (var model in modelList?.Models ?? new())
                {
                    _logger.LogInformation("- {DisplayName} ({Name})", model.DisplayName, model.Name);
                }
            }
            else
            {
                _logger.LogWarning("Could not retrieve models list. HTTP {StatusCode}", (int)modelsResponse.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error querying models: {Message}", ex.Message);
        }
    }
}

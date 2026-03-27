using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using squad_func.Models.AI;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
    }

    public async Task<string?> GenerateContentAsync(string userPrompt)
    {
        string defaultInstructions = """
Role & Objective: You are a specialized Mens Soccer Data Extraction Agent. Your sole purpose is to browse the web to find real-time fixture, score, and squad information for a requested soccer league and return that data in a strict JSON format.

Operational Workflow:
1. League Identification: Confirm the target league and the current date/season context.
2. Web Search: Access trusted sports databases (e.g., Transfermarkt, SofaScore, BBC Sport, or official league sites).
3. Data Extraction:
   - Fixtures: Capture Date, Time, Home Team, and Away Team.
   - Scores: Capture current score and match status (e.g., "FT", "75'", "Postponed").
   - Squads: If within 60 minutes of kickoff, extract the "Starting XI" and "Substitutes". Otherwise, return "Lineups not yet released".
4. Temporal Validation: Always compare found data against the current date to ensure seasonal accuracy.
5. Share data source link for verification if possible


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

        Console.WriteLine("Sending request to Gemini API...");
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error HTTP {(int)response.StatusCode}: {errorContent}");
            return null;
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseJson);

        return geminiResponse?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
    }

    public async Task ReportAvailableModelsAsync()
    {
        Console.WriteLine("Querying available models...");
        string modelsUrl = $"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}";
        try
        {
            var modelsResponse = await _httpClient.GetAsync(modelsUrl);
            if (modelsResponse.IsSuccessStatusCode)
            {
                string modelsJson = await modelsResponse.Content.ReadAsStringAsync();
                var modelList = JsonSerializer.Deserialize<ModelListResponse>(modelsJson);
                Console.WriteLine("\n--- Available Models ---");
                foreach (var model in modelList?.Models ?? new())
                {
                    Console.WriteLine($"- {model.DisplayName} ({model.Name})");
                }
                Console.WriteLine("------------------------\n");
            }
            else
            {
                Console.WriteLine($"Warning: Could not retrieve models list. HTTP {(int)modelsResponse.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Error querying models: {ex.Message}");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using squad_func.Models.AI;

public class GeminiService(HttpClient httpClient, ILogger<GeminiService> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly string _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
    private const string _agentModel = "gemini-3.1-flash-lite-preview";
    private readonly ILogger<GeminiService> _logger = logger;
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Generates content for a given league and formatted date.
    /// </summary>
    /// <param name="league">The name of the league.</param>
    /// <param name="formattedDate">The formatted date.</param>
    /// <returns>A string containing the generated content.</returns>
    public async Task<string?> GenerateContentAsync(string league, string formattedDate)
    {
        string promptFilePath = Path.Combine(AppContext.BaseDirectory, "prompts/agent-prompt.md");
        string template = await File.ReadAllTextAsync(promptFilePath);
        string userPrompt = template.Replace("{LEAGUE}", league).Replace("{FORMATTED_DATE}", formattedDate);

        var requestBody = BuildBaseRequestBody(userPrompt);
        string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_agentModel}:generateContent?key={_apiKey}";

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
        //_logger.LogInformation("Gemini API Response: {ResponseJson}", responseJson);
        return responseJson;
    }

    /// <summary>
    /// Gets the history of a team for a given season.
    /// </summary>
    /// <param name="team">The name of the team.</param>
    /// <param name="season">The season, in the format "yyyy/yyyy".</param>
    /// <returns>A string containing the history of the team for the given season.</returns>
    public async Task<string> GetHistoryAsync(string team, string season)
    {
        _logger.LogInformation("Getting history for {Team} {Season}", team, season);

        // Add a try/catch flow
        try
        {

            string promptFilePath = Path.Combine(AppContext.BaseDirectory, "prompts/history.md");
            string template = await File.ReadAllTextAsync(promptFilePath);
            string userPrompt = template.Replace("{TEAM}", team).Replace("{SEASON}", season);

            var requestBody = BuildBaseRequestBody(userPrompt);

            string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_agentModel}:generateContent?key={_apiKey}";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(200));
            _logger.LogInformation("Sending request to Gemini API...");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error HTTP {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting history for {Team} {Season}: {Error}", team, season, ex.Message);
            return null;
        }
    }

    public async Task<string?> GetSingleMatchHistoryAsync(string team, string season, string matchDate)
    {
        _logger.LogInformation("Getting history for {Team} {Season} {MatchDate}", team, season, matchDate);

        // Add a try/catch flow
        try
        {
            string promptFilePath = Path.Combine(AppContext.BaseDirectory, "prompts/team_fixture_prompt.md");
            string template = await File.ReadAllTextAsync(promptFilePath);

            string finalPrompt = template
            .Replace("[INSERT TEAM NAME]", team)
            .Replace("[INSERT LEAGUE/SEASON, e.g., 2024/25 Premier League]", season)
            .Replace("[INSERT DATE, e.g., November 12, 2024]", matchDate);

            //_logger.LogInformation("Sending request to Gemini API with prompt: {Prompt}", finalPrompt);

            var requestBody = BuildBaseRequestBody(finalPrompt);

            string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_agentModel}:generateContent?key={_apiKey}";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(200));
            _logger.LogInformation("Sending request to Gemini API...");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Error HTTP {StatusCode}: {ErrorContent}", (int)response.StatusCode, errorContent);
                return null;
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            return responseJson;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting history for {Team} {MatchDate}: {Error}", team, matchDate, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Builds the base request body for the Gemini API.
    /// </summary>
    /// <param name="userPrompt">The user prompt.</param>
    /// <returns>The request body.</returns>
    private static global::System.Object BuildBaseRequestBody(string userPrompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = userPrompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                // Latency is directly proportional to the number of tokens generated.
                // Use the max_output_tokens parameter to restrict the length of the response
                // double the output tokens does not seem to have an impact on the response time
                maxOutputTokens = 12288
            }
        };
        return requestBody;
    }
}

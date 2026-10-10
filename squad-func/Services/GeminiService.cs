using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

public class GeminiService(HttpClient httpClient, ILogger<GeminiService> logger)
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly string _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? string.Empty;
    private const string _agentModel = "gemini-3.1-flash-lite";
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
        return responseJson;
    }

    /// <summary>
    /// Builds the base request body for the Gemini API.
    /// </summary>
    /// <param name="userPrompt">The user prompt.</param>
    /// <returns>The request body.</returns>
    private static object BuildBaseRequestBody(string userPrompt)
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

    /// <summary>
    /// Gets the player photo prompt for a given last name.
    /// This method reads a prompt template from a file, replaces a placeholder with the provided last
    /// </summary>
    /// <param name="lastname"></param>
    /// <returns></returns>
    public async Task<string?> GetPlayerPhotoPrompt(string lastname)
    {
        try
        {
            string promptFilePath = Path.Combine(AppContext.BaseDirectory, "prompts/playername-prompt.md");
            string template = await File.ReadAllTextAsync(promptFilePath);

            string finalPrompt = template
            .Replace("[INSERT PLAYER NAME HERE]", lastname);
            var requestBody = BuildBaseRequestBody(finalPrompt);

            string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_agentModel}:generateContent?key={_apiKey}";

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
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
            _logger.LogError("Error getting image data for GetPlayerPhotoPrompt: {Error}", ex.Message);
            return null;
        }
    }
}

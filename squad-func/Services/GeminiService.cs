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
    private readonly ILogger<GeminiService> _logger = logger;
    private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<string?> GenerateContentAsync(string league, string formattedDate)
    {
        string promptFilePath = Path.Combine(AppContext.BaseDirectory, "agent-prompt.md");
        string template = await File.ReadAllTextAsync(promptFilePath);
        string userPrompt = template.Replace("{LEAGUE}", league).Replace("{FORMATTED_DATE}", formattedDate);

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
                temperature = 0.2
            }
        };

        string json = JsonSerializer.Serialize(requestBody, _serializerOptions);

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

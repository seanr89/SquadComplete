using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// 1. Load environment variables from .env file
string envPath = Path.Combine(AppContext.BaseDirectory, ".env");
// Also check the project directory (working directory)
if (!File.Exists(envPath))
{
    envPath = ".env";
}

LoadEnv(envPath);

var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
var model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash";
var baseUrl = Environment.GetEnvironmentVariable("GEMINI_API_URL") ?? "https://generativelanguage.googleapis.com";

if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GEMINI_API_KEY")
{
    Console.Error.WriteLine("Error: GEMINI_API_KEY is not set or has the placeholder value in .env file.");
    Console.Error.WriteLine("Please edit the .env file with your valid Gemini API key from Google AI Studio.");
    return;
}

// 2. Read prompt from prompts/cl_prompt.md
var promptPath = Path.Combine("prompts", "cl_prompt.md");
if (!File.Exists(promptPath))
{
    Console.Error.WriteLine($"Error: Prompt file not found at '{promptPath}'.");
    return;
}

Console.WriteLine($"Reading prompt from: {promptPath}...");
var promptContent = await File.ReadAllTextAsync(promptPath);

// 3. Construct Gemini API Endpoint URL
string requestUrl;
if (baseUrl.Contains("{model}"))
{
    requestUrl = baseUrl.Replace("{model}", model);
}
else if (baseUrl.Contains("models/"))
{
    requestUrl = baseUrl;
}
else
{
    baseUrl = baseUrl.TrimEnd('/');
    if (baseUrl.EndsWith("/v1beta") || baseUrl.Contains("/v1"))
    {
        requestUrl = $"{baseUrl}/models/{model}:generateContent";
    }
    else
    {
        requestUrl = $"{baseUrl}/v1beta/models/{model}:generateContent";
    }
}

Console.WriteLine($"Target API URL: {requestUrl}");
Console.WriteLine($"Target Model:   {model}");

// 4. Construct payload
var payload = new
{
    contents = new[]
    {
        new
        {
            parts = new[]
            {
                new { text = promptContent }
            }
        }
    },
    generationConfig = new
    {
        responseMimeType = "application/json"
    }
};

var jsonPayload = JsonSerializer.Serialize(payload);

// 5. Send POST request
using var httpClient = new HttpClient();
using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
request.Headers.Add("x-goog-api-key", apiKey);
request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

Console.WriteLine("Sending request to Gemini API...");
try
{
    var response = await httpClient.SendAsync(request);
    var responseString = await response.Content.ReadAsStringAsync();

    if (!response.IsSuccessStatusCode)
    {
        Console.Error.WriteLine($"Error: API call failed with status code {(int)response.StatusCode} ({response.StatusCode})");
        TryPrintApiError(responseString);
        return;
    }

    // 6. Parse response JSON
    using var responseDoc = JsonDocument.Parse(responseString);
    var root = responseDoc.RootElement;

    if (root.TryGetProperty("error", out var errorEl))
    {
        var message = errorEl.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : "Unknown error";
        Console.Error.WriteLine($"Error: {message}");
        return;
    }

    if (root.TryGetProperty("candidates", out var candidates) &&
        candidates.ValueKind == JsonValueKind.Array &&
        candidates.GetArrayLength() > 0)
    {
        var candidate = candidates[0];
        if (candidate.TryGetProperty("content", out var content) &&
            content.TryGetProperty("parts", out var parts) &&
            parts.ValueKind == JsonValueKind.Array &&
            parts.GetArrayLength() > 0)
        {
            var rawText = parts[0].GetProperty("text").GetString();
            if (string.IsNullOrEmpty(rawText))
            {
                Console.Error.WriteLine("Error: API response text is empty.");
                return;
            }

            // 7. Sanitize and write JSON to output.json
            var sanitizedJson = SanitizeJson(rawText);
            try
            {
                using var outputDoc = JsonDocument.Parse(sanitizedJson);
                var prettyJson = JsonSerializer.Serialize(outputDoc.RootElement, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync("output.json", prettyJson);
                Console.WriteLine("Success! Saved response to: output.json");
            }
            catch (JsonException)
            {
                await File.WriteAllTextAsync("output.json", sanitizedJson);
                Console.WriteLine("Warning: Response content could not be verified as valid JSON. Saved raw response to: output.json");
            }
        }
        else
        {
            Console.Error.WriteLine("Error: Response does not contain content parts.");
        }
    }
    else
    {
        Console.Error.WriteLine("Error: Response candidates array is empty or missing.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Exception: {ex.Message}");
}

// Environment Loading Helper
static void LoadEnv(string filePath)
{
    if (!File.Exists(filePath)) return;
    
    foreach (var line in File.ReadAllLines(filePath))
    {
        var trimmedLine = line.Trim();
        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
            continue;

        var parts = trimmedLine.Split('=', 2);
        if (parts.Length != 2) continue;

        var key = parts[0].Trim();
        var val = parts[1].Trim();

        // Strip quotes if present
        if (val.StartsWith("\"") && val.EndsWith("\"") && val.Length >= 2)
            val = val.Substring(1, val.Length - 2);
        else if (val.StartsWith("'") && val.EndsWith("'") && val.Length >= 2)
            val = val.Substring(1, val.Length - 2);

        Environment.SetEnvironmentVariable(key, val);
    }
}

// JSON Sanitization Helper to strip potential markdown code fences
static string SanitizeJson(string rawText)
{
    var trimmed = rawText.Trim();
    if (trimmed.StartsWith("```"))
    {
        int firstNewLine = trimmed.IndexOf('\n');
        if (firstNewLine != -1)
        {
            trimmed = trimmed.Substring(firstNewLine).Trim();
        }
        else
        {
            trimmed = trimmed.Substring(3).Trim();
        }

        if (trimmed.EndsWith("```"))
        {
            trimmed = trimmed.Substring(0, trimmed.Length - 3).Trim();
        }
    }
    return trimmed;
}

// Error printer helper
static void TryPrintApiError(string responseBody)
{
    try
    {
        using var doc = JsonDocument.Parse(responseBody);
        var root = doc.RootElement;
        if (root.TryGetProperty("error", out var errorEl))
        {
            var message = errorEl.TryGetProperty("message", out var msgEl) ? msgEl.GetString() : null;
            var status = errorEl.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
            if (message != null)
            {
                Console.Error.WriteLine($"Details: {message} ({status ?? "No status"})");
                return;
            }
        }
    }
    catch
    {
        // Fallback to printing raw output if it cannot be parsed as JSON
    }
    Console.Error.WriteLine($"Raw Response: {responseBody}");
}

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using squad_func.Services;
using CsvHelper;
using System.Text.Json;

namespace Squad.Function;

public class AgentFixtures
{
    private readonly ILogger _logger;
    private readonly SquadContext _context;
    private readonly IApiService _apiService;
    // add dbservice
    private readonly DatabaseService _databaseService;

    public AgentFixtures(ILoggerFactory loggerFactory, SquadContext context,
    IApiService apiService, DatabaseService databaseService)
    {
        _logger = loggerFactory.CreateLogger<DailyFixtures>();
        _context = context;
        _apiService = apiService;
        _databaseService = databaseService;
    }

    [Function("AgentFixtures")]
    public async Task Run([TimerTrigger("0 0 10 * * *")] TimerInfo myTimer)
    {
        //
    }

    private static string ConvertResponseToJson(string aiText)
    {
        // --- Extract and Save JSON ---
        try
        {
            string jsonContent = string.Empty;
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

            if (!string.IsNullOrEmpty(jsonContent))
            {
                using (JsonDocument.Parse(jsonContent)) // Validate JSON
                {
                    // string jsonFilename = $"{selectedLeague?.Replace(" ", "-") ?? "unknown-league"}_{previousDate}_{timestamp}.json";
                    // string jsonFilePath = Path.Combine(responsesDir, jsonFilename);
                    // await File.WriteAllTextAsync(jsonFilePath, jsonContent);
                    // Console.WriteLine($"JSON data extracted and saved to {jsonFilePath}");
                }
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
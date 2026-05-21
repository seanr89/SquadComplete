using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace Squad.Function;

public class CleanupGameRecords(ILoggerFactory loggerFactory, SquadContext context)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<CleanupGameRecords>();
    private readonly SquadContext _context = context;

    /// <summary>
    /// Function to clean up game records older than a specific number of days.
    /// Scheduled to run at 2am every Monday.
    /// NCRONTAB format: {second} {minute} {hour} {day} {month} {day-of-week}
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("CleanupGameRecords")]
    public async Task Run([TimerTrigger("0 0 2 * * 1")] TimerInfo myTimer)
    {
        _logger.LogInformation("CleanupGameRecords timer trigger function started at: {Time}", DateTime.UtcNow);

        // Retrieve the cleanup threshold in days, defaulting to 60 days
        int cleanupDays = 60;
        string? cleanupDaysSetting = Environment.GetEnvironmentVariable("GameRecordsCleanupDays");

        if (!string.IsNullOrEmpty(cleanupDaysSetting) && int.TryParse(cleanupDaysSetting, out int parsedDays))
        {
            cleanupDays = parsedDays;
        }

        _logger.LogInformation("Cleaning up game records older than {Days} days.", cleanupDays);

        try
        {
            // Compute the cutoff date (records with game_date less than this cutoff will be deleted)
            DateTime cutoffDate = DateTime.UtcNow.Date.AddDays(-cleanupDays);
            _logger.LogInformation("Game records with a game date before {CutoffDate} will be deleted.", cutoffDate);

            // Perform bulk deletion directly on the database
            int deletedRecords = await _context.GameRecords
                .Where(g => g.GameDate < cutoffDate)
                .ExecuteDeleteAsync();

            _logger.LogInformation("Successfully deleted {Count} game records.", deletedRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during game records cleanup.");
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {Message}", ex.InnerException.Message);
            }
        }
    }
}

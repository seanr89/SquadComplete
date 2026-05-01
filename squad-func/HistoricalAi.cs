using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;
using squad_func.Models;
using Microsoft.EntityFrameworkCore;

namespace Squad.Function;

public class HistoricalAi(ILoggerFactory loggerFactory,
    GeminiService geminiService, StorageService storageService, SquadContext context)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<HistoricalAi>();
    private readonly GeminiService _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));

    [Function("HistoricalAi")]
    public async Task Run([TimerTrigger("0 0 10 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("HistoricalAi started");

        var season = await _context.TeamSeasons
        .Include(x => x.Team)
        .Include(x => x.Season)
        .Where(ts => !ts.DataRequested)
        .AsNoTracking()
        .Take(1)
        .ToListAsync();


        if (season != null)
        {
            var team = season.First().Team;
            var seasonInfo = season.First().Season;

            _logger.LogInformation("HistoricalAi started for {Team} {Season}", team.Name, seasonInfo.Name);

            var geminiData = await _geminiService.GetHistoryAsync(team.Name, seasonInfo.Name);

            if (geminiData != null)
            {
                var blobName = $"{team.Name}_{seasonInfo.Name}_history.json";
                await _storageService.UploadToStorage(geminiData, blobName, "squad-history");
            }
            else
            {
                _logger.LogError("HistoricalAi failed for {Team} {Season}", team.Name, seasonInfo.Name);
            }
        }
    }
}

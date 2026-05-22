using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;
using squad_func.Models;
using Microsoft.EntityFrameworkCore;

namespace Squad.Function;

public class FullSeasonAISearch(ILoggerFactory loggerFactory,
    GeminiService geminiService, StorageService storageService, SquadContext context)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<FullSeasonAISearch>();
    private readonly GeminiService _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));

    [Function("FullSeasonAISearch")]
    public async Task Run([TimerTrigger("0 0 21 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("FullSeasonAISearch started");

        var season = await _context.TeamSeasons
        .Include(x => x.Team)
        .Include(x => x.Season)
        .Where(ts => !ts.DataRequested)
        .Where(ts => ts.Team.Active)
        .OrderBy(x => x.Team.CreatedAt)
        .AsNoTracking()
        .Take(1)
        .ToListAsync();


        if (season != null)
        {
            var team = season.First().Team;
            var seasonInfo = season.First().Season;

            var geminiData = await _geminiService.GetHistoryAsync(team.Name, seasonInfo.Name);
            if (geminiData != null)
            {
                string seasonInfoName = seasonInfo.Name.Replace("/", "-");
                var blobName = $"{team.Name}_{seasonInfoName}_history.json";
                var jsonFormatted = SeasonDataProcessor.ProcessAndRemoveLosses(geminiData, team.Name);
                if (jsonFormatted != null)
                {
                    await _storageService.UploadToStorage(jsonFormatted, blobName, "squad-history");

                    // update the data requested flag
                    var teamSeason = await _context.TeamSeasons
                        .Where(ts => ts.TeamId == team.Id && ts.SeasonId == seasonInfo.Id)
                        .FirstOrDefaultAsync();

                    if (teamSeason != null)
                    {
                        teamSeason.DataRequested = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            else
            {
                _logger.LogError("HistoricalAi failed for {Team} {Season}", team.Name, seasonInfo.Name);
            }
        }
    }
}

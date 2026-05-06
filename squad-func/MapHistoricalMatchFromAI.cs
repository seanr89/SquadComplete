

using System.Linq;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Squad.Function.Models.AI;
using squad_func.Models;
using squad_func.Services;

namespace Squad.Function;

public class MapHistoricalMatchFromAI(ILoggerFactory loggerFactory, SquadContext context,
 StorageService storageService)
{

    private readonly ILogger _logger = loggerFactory.CreateLogger<SingleMatchHistoricalSearch>();
    private readonly SquadContext _context = context;
    private readonly StorageService _storageService = storageService;

    /// <summary>
    /// Function to refresh player and team info
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("MapHistoricalMatchFromAI")]
    public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo myTimer)
    {
        // step 1. lets run and see if there is a historical record/file to search
        if (await _storageService.IsContainerEmpty("ai-team-single") == true)
        {
            _logger.LogInformation("Historical Match container is empty, nothing to do");
            return;
        }

        // step 2. read down file from storage
        var blobs = await _storageService.GetBlobs("ai-team-single");
        var blob = blobs.First();
        var blobData = await _storageService.ReadFromStorage(blob, "ai-team-single");

        //Now we can map the data
        var data = JsonSerializer.Deserialize<MatchDetails>(blobData);
        if (data == null)
        {
            _logger.LogError("Error deserializing blob data with error {Error}", blobData);
            return;
        }

        // now we need to try and build a fixture and player stats records.
        var matchingFixtureTeam = await _context.Fixtures.Where(x => x.HomeTeamName == data.HomeTeam.Name
            || x.AwayTeamName == data.AwayTeam.Name).AsNoTracking().FirstOrDefaultAsync();

        // first lets try and map the home team and away team.
        if (matchingFixtureTeam == null)
        {
            _logger.LogError("Error deserializing blob data with error {Error}", blobData);
            return;
        }

        var newFixture = new Fixture
        {
            HomeTeamId = matchingFixtureTeam.HomeTeamId,
            HomeTeamName = data.HomeTeam.Name,
            AwayTeamName = data.AwayTeam.Name,
            HomeGoalCount = int.Parse(data.MatchMetadata.FinalScore.Split('-')[0]),
            AwayGoalCount = int.Parse(data.MatchMetadata.FinalScore.Split('-')[1]),
        };



    }

}
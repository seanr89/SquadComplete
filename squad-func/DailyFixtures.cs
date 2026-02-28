using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class DailyFixtures
{
    private readonly ILogger _logger;
    private readonly IApiService _apiService;
    private readonly IDatabaseService _databaseService;

    public DailyFixtures(ILoggerFactory loggerFactory, IApiService apiService,
         IDatabaseService databaseService)
    {
        _logger = loggerFactory.CreateLogger<DailyFixtures>();
        _apiService = apiService;
        _databaseService = databaseService;
    }

    [Function("DailyFixtures")]
    public void Run([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        // step 1. get all leagues we allow
        var leagues = _databaseService.GetLeaguesAsync();
        var date = DateTime.Now.AddDays(-1);
        foreach (var league in leagues)
        {
            _logger.LogInformation("Processing league {leagueId}", league.Id);
            //var fixtures = _apiService.GetFixturesForLeague(league.Id, DateTime.Now);
        }
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
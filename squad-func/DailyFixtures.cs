using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using squad_func.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using squad_func.Services;

namespace Squad.Function;

public class DailyFixtures
{
    private readonly ILogger _logger;
    private readonly SquadContext _context;
    private readonly IApiService _apiService;
    // add dbservice
    private readonly DatabaseService _databaseService;

    public DailyFixtures(ILoggerFactory loggerFactory, SquadContext context, 
    IApiService apiService, DatabaseService databaseService)
    {
        _logger = loggerFactory.CreateLogger<DailyFixtures>();
        _context = context;
        _apiService = apiService;
        _databaseService = databaseService;
    }

    [Function("DailyFixtures")]
    public async Task Run([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        // step 1. get all leagues we allow
        // var leagues = await _databaseService.GetLeaguesAsync();
        // var date = DateTime.Now.AddDays(-1);
        // foreach (var league in leagues)
        // {
        //     _logger.LogInformation("Processing league {leagueId}", league.Id);
        //     //var fixtures = _apiService.GetFixturesForLeague(league.Id, DateTime.Now);
        // }
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
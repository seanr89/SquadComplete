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
        //_logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        // step 1. fixure info
        var setFixturesWithoutScores = await _context.Fixtures
            .Where(f => f.HomeGoalCount == null && f.AwayGoalCount == null)
            .Take(10)
            .ToListAsync();

        if (setFixturesWithoutScores.Count == 0)
        {
            _logger.LogInformation("No fixtures without scores found");
            return;
        }

        foreach (var fixture in setFixturesWithoutScores)
        {
            await _apiService.GetFixtureDataAsync(fixture.Id);
            //todo: add player stats
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
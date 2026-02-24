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
    private readonly IDatabaseService _databaseService;

    public DailyFixtures(ILoggerFactory loggerFactory, SquadContext context, 
    IApiService apiService, IDatabaseService databaseService)
    {
        _logger = loggerFactory.CreateLogger<DailyFixtures>();
        _context = context;
        _apiService = apiService;
        _databaseService = databaseService;
    }

    /// <summary>
    /// Azure Function trigger that runs on a schedule to fetch player statistics for fixtures that don't have them yet.
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("DailyFixtures")]
    public async Task Run([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}",
            DateTime.Now);

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}
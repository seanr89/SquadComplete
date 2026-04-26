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

public class RefreshPlayerInfo(ILoggerFactory loggerFactory, SquadContext context,
IApiService apiService, DatabaseService databaseService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RefreshPlayerInfo>();
    private readonly SquadContext _context = context;
    private readonly IApiService _apiService = apiService;
    // add dbservice
    private readonly DatabaseService _databaseService = databaseService;

    /// <summary>
    /// Function to refresh player and team info
    /// </summary>
    /// <param name="myTimer">The timer trigger info.</param>
    [Function("RefreshPlayerInfo")]
    public async Task Run([TimerTrigger("0 0 6 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("RefreshPlayerInfo started");


    }
}
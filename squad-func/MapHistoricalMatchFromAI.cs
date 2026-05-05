

using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
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
    }

}
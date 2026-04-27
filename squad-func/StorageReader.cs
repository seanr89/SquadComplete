using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Models;
using squad_func.Services;
using System.Text.Json;

namespace Squad.Function;

public class StorageReader(ILoggerFactory loggerFactory,
    StorageService storageService,
    IAgentMappingService agentFixtureMapperService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StorageReader>();
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    private readonly IAgentMappingService _agentMappingService = agentFixtureMapperService ?? throw new ArgumentNullException(nameof(agentFixtureMapperService));

    [Function("StorageReader")]
    public async Task Run([TimerTrigger("0 30 10-12 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("StorageReader started");
        try
        {
            var blobs = await _storageService.GetBlobs("agent-fixtures");
            if (blobs.Count > 0)
            {
                foreach (var blob in blobs)
                {
                    var data = await _storageService.ReadFromStorage(blob, "agent-fixtures");
                    if (!string.IsNullOrEmpty(data))
                    {
                        try
                        {
                            // read data with blob name and and convert data from string/JSON to AgentFixture class
                            var fixture = JsonSerializer.Deserialize<AgentFixture>(data);
                            if (fixture == null) continue;

                            //await _agentMappingService.ProcessAgentFixtureAsync(fixture);
                        }
                        catch (JsonException ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize JSON for blob: {BlobName}", blob);
                            //TODO lets move the file if damaged to error
                            await _storageService.MoveBlob(blob, "agent-fixtures", "errors");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during fixture stats retrieval.");
        }
    }
}
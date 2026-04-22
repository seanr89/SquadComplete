using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;
using System.Text.Json;

namespace Squad.Function;

public class StorageReader(ILoggerFactory loggerFactory,
    StorageService storageService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<StorageReader>();
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    [Function("StorageReader")]
    public async Task Run([TimerTrigger("0 30 10 * * *")] TimerInfo myTimer)
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
                        // trim leading and trailing whitespace
                        data = data.Trim();
                        // replace multiple spaces with a single space
                        data = System.Text.RegularExpressions.Regex.Replace(data, @"\s+", " ");
                        _logger.LogInformation("Data: {Data}", data);
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
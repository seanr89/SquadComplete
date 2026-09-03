using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Services;

namespace Squad.Function;

public class GetPlayerImage(ILoggerFactory loggerFactory, StorageService storageService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GetPlayerImage>();
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));

    [Function("GetPlayerImage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player-image/{name?}")] HttpRequest req,
        string? name)
    {
        return await HandleRequestAsync(req, name);
    }

    [Function("GetPlayerImageByPlayerRoute")]
    public async Task<IActionResult> RunByPlayerRoute(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players/{name}/image")] HttpRequest req,
        string? name)
    {
        return await HandleRequestAsync(req, name);
    }

    private async Task<IActionResult> HandleRequestAsync(HttpRequest req, string? routeName)
    {
        string? playerName = !string.IsNullOrWhiteSpace(routeName)
            ? routeName
            : req.Query["name"].FirstOrDefault()
              ?? req.Query["playerName"].FirstOrDefault()
              ?? req.Query["player"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(playerName))
        {
            _logger.LogWarning("Player image request rejected: missing player name.");
            return new BadRequestObjectResult(new
            {
                error = "Player name is required. Specify it in the route (e.g., /api/player-image/{name}) or via 'name' query parameter."
            });
        }

        // Optional query parameter to override target container name
        string? containerName = req.Query["container"].FirstOrDefault();

        _logger.LogInformation("Querying image for player '{PlayerName}' in container '{ContainerName}'...",
            playerName, containerName ?? "(default: playersname)");

        try
        {
            var imageResult = await _storageService.GetPlayerImageAsync(playerName, containerName);

            if (imageResult == null)
            {
                _logger.LogInformation("No image found for player '{PlayerName}'.", playerName);
                return new NotFoundObjectResult(new
                {
                    error = $"Image for player '{playerName}' was not found.",
                    player = playerName,
                    container = containerName ?? "playersname"
                });
            }

            var (content, contentType, blobName) = imageResult.Value;

            // Add caching headers for optimal performance
            req.HttpContext.Response.Headers.CacheControl = "public, max-age=86400";
            req.HttpContext.Response.Headers.ETag = $"\"{blobName}\"";

            _logger.LogInformation("Returning image for player '{PlayerName}' (blob: '{BlobName}', type: {ContentType}, size: {Size} bytes).",
                playerName, blobName, contentType, content.Length);

            return new FileContentResult(content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving image for player '{PlayerName}'.", playerName);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

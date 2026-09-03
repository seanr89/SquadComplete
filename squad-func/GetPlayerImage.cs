using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;
using squad_func.Services;

namespace Squad.Function;

public class GetPlayerImage(ILoggerFactory loggerFactory, StorageService storageService, SquadContext context)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GetPlayerImage>();
    private readonly StorageService _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
    private readonly SquadContext _context = context ?? throw new ArgumentNullException(nameof(context));

    [Function("GetPlayerImage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "player-image/{nameOrId?}")] HttpRequest req,
        string? nameOrId)
    {
        return await HandleRequestAsync(req, nameOrId);
    }

    [Function("GetPlayerImageByPlayerRoute")]
    public async Task<IActionResult> RunByPlayerRoute(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players/{nameOrId}/image")] HttpRequest req,
        string? nameOrId)
    {
        return await HandleRequestAsync(req, nameOrId);
    }

    [Function("GetPlayerImageByIdAndName")]
    public async Task<IActionResult> RunByIdAndName(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "players/{id}/{name}/image")] HttpRequest req,
        string? id,
        string? name)
    {
        return await HandleRequestAsync(req, name, id);
    }

    private async Task<IActionResult> HandleRequestAsync(HttpRequest req, string? routeNameOrId, string? routeId = null)
    {
        string? queryId = req.Query["id"].FirstOrDefault()
            ?? req.Query["playerId"].FirstOrDefault();

        string? queryName = req.Query["name"].FirstOrDefault()
            ?? req.Query["playerName"].FirstOrDefault()
            ?? req.Query["player"].FirstOrDefault();

        string? resolvedId = routeId ?? queryId;
        string? resolvedName = null;

        if (!string.IsNullOrWhiteSpace(routeNameOrId))
        {
            string unescaped = Uri.UnescapeDataString(routeNameOrId).Trim();

            // Check if routeNameOrId is a pure numeric ID (e.g. "511933")
            if (int.TryParse(unescaped, out _))
            {
                resolvedId ??= unescaped;
                resolvedName = queryName;
            }
            // Check if routeNameOrId is in format '{id}_{name}' (e.g. '511933_Stephane Henchoz.jpg')
            else
            {
                int sepIdx = unescaped.IndexOfAny(['_', '-']);
                if (sepIdx > 0 && int.TryParse(unescaped[..sepIdx], out _))
                {
                    resolvedId ??= unescaped[..sepIdx];
                    resolvedName = unescaped[(sepIdx + 1)..];
                }
                else
                {
                    resolvedName = unescaped;
                }
            }
        }
        else
        {
            resolvedName = queryName;
        }

        if (string.IsNullOrWhiteSpace(resolvedId) && string.IsNullOrWhiteSpace(resolvedName))
        {
            _logger.LogWarning("Player image request rejected: missing player name or ID.");
            return new BadRequestObjectResult(new
            {
                error = "Player ID or name is required. Specify it via route (e.g., /api/player-image/511933 or /api/player-image/511933_Stephane Henchoz.jpg) or query parameters (?id=... or ?name=...)."
            });
        }

        // If player name is known but ID is missing, query the database for ApiId or Id
        if (string.IsNullOrWhiteSpace(resolvedId) && !string.IsNullOrWhiteSpace(resolvedName))
        {
            try
            {
                string searchName = Path.GetFileNameWithoutExtension(resolvedName).Trim();
                var dbPlayer = await _context.Players
                    .AsNoTracking()
                    .Where(p => p.Name.ToLower() == searchName.ToLower())
                    .FirstOrDefaultAsync();

                if (dbPlayer != null)
                {
                    if (dbPlayer.ApiId.HasValue && dbPlayer.ApiId.Value > 0)
                    {
                        resolvedId = dbPlayer.ApiId.Value.ToString();
                    }
                    else if (dbPlayer.Id > 0)
                    {
                        resolvedId = dbPlayer.Id.ToString();
                    }
                    _logger.LogInformation("Resolved player '{PlayerName}' to ID '{PlayerId}' from database.",
                        resolvedName, resolvedId);
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "Could not lookup player by name '{PlayerName}' in database. Proceeding with storage search.", resolvedName);
            }
        }
        // If ID is known but player name is missing, attempt database lookup to retrieve player name
        else if (!string.IsNullOrWhiteSpace(resolvedId) && string.IsNullOrWhiteSpace(resolvedName) && int.TryParse(resolvedId, out int numericId))
        {
            try
            {
                var dbPlayer = await _context.Players
                    .AsNoTracking()
                    .Where(p => p.ApiId == numericId || p.Id == numericId)
                    .FirstOrDefaultAsync();

                if (dbPlayer != null && !string.IsNullOrWhiteSpace(dbPlayer.Name))
                {
                    resolvedName = dbPlayer.Name;
                    _logger.LogInformation("Resolved player ID '{PlayerId}' to name '{PlayerName}' from database.",
                        resolvedId, resolvedName);
                }
            }
            catch (Exception dbEx)
            {
                _logger.LogWarning(dbEx, "Could not lookup player by ID '{PlayerId}' in database. Proceeding with storage search.", resolvedId);
            }
        }

        // Optional query parameter to override target container name
        string? containerName = req.Query["container"].FirstOrDefault();

        _logger.LogInformation("Querying image for Player ID: '{PlayerId}', Name: '{PlayerName}' in container '{ContainerName}'...",
            resolvedId ?? "N/A", resolvedName ?? "N/A", containerName ?? "(default: playersname)");

        try
        {
            var imageResult = await _storageService.GetPlayerImageAsync(resolvedName, resolvedId, containerName);

            if (imageResult == null)
            {
                _logger.LogInformation("No image found for Player ID: '{PlayerId}', Name: '{PlayerName}'.",
                    resolvedId ?? "N/A", resolvedName ?? "N/A");

                return new NotFoundObjectResult(new
                {
                    error = "Player image was not found.",
                    playerId = resolvedId,
                    playerName = resolvedName,
                    container = containerName ?? "playersname"
                });
            }

            var (content, contentType, blobName) = imageResult.Value;

            // Add caching headers for optimal performance
            req.HttpContext.Response.Headers.CacheControl = "public, max-age=86400";
            req.HttpContext.Response.Headers.ETag = $"\"{blobName}\"";

            _logger.LogInformation("Returning image for Player ID: '{PlayerId}', Name: '{PlayerName}' (blob: '{BlobName}', type: {ContentType}, size: {Size} bytes).",
                resolvedId ?? "N/A", resolvedName ?? "N/A", blobName, contentType, content.Length);

            return new FileContentResult(content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error retrieving image for Player ID: '{PlayerId}', Name: '{PlayerName}'.",
                resolvedId ?? "N/A", resolvedName ?? "N/A");

            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

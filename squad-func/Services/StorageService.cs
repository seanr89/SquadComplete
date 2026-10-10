
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace squad_func.Services;

public class StorageService(ILogger<StorageService> logger, IConfiguration configuration)
{
    private readonly ILogger<StorageService> _logger = logger;
    private readonly IConfiguration _configuration = configuration;

    /// <summary>The only blob container this service reads from.</summary>
    private const string PlayerImageContainer = "playersname";

    /// <summary>
    /// Gets a configured BlobServiceClient instance using FixtureStorage or AzureWebJobsStorage.
    /// </summary>
    public BlobServiceClient GetBlobServiceClient()
    {
        string? connectionString = _configuration["FixtureStorage"]
            ?? Environment.GetEnvironmentVariable("FixtureStorage")
            ?? _configuration["AzureWebJobsStorage"]
            ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage' or 'AzureWebJobsStorage'.");
            throw new InvalidOperationException("Storage connection string is not configured.");
        }

        return new BlobServiceClient(connectionString);
    }

    /// <summary>
    /// Searches for and downloads a player's image from Azure Blob Storage.
    /// Supports images formatted like '{playerId}_{playerName}.jpg' (e.g. '511933_Stephane Henchoz.jpg'),
    /// querying by player ID, player name, or both, as well as playersname variations.
    /// </summary>
    /// <param name="playerName">The player name to query (optional if playerId is supplied).</param>
    /// <param name="playerId">The player ID to query (optional if playerName is supplied or included in input).</param>
    /// <returns>A tuple of the image bytes, MIME content type, and the resolved blob name, or null if not found.</returns>
    public async Task<(byte[] Content, string ContentType, string BlobName)?> GetPlayerImageAsync(
        string? playerName,
        string? playerId = null)
    {
        // Parse input if playerName contains ID prefix (e.g., '511933_Stephane Henchoz.jpg' or '511933')
        if (!string.IsNullOrWhiteSpace(playerName))
        {
            playerName = Uri.UnescapeDataString(playerName).Trim();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                if (int.TryParse(playerName, out _))
                {
                    playerId = playerName;
                    playerName = null;
                }
                else
                {
                    int sepIdx = playerName.IndexOfAny(['_', '-']);
                    if (sepIdx > 0 && int.TryParse(playerName[..sepIdx], out _))
                    {
                        playerId = playerName[..sepIdx];
                        playerName = playerName[(sepIdx + 1)..];
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(playerId))
        {
            playerId = Uri.UnescapeDataString(playerId).Trim();
        }

        if (string.IsNullOrWhiteSpace(playerName) && string.IsNullOrWhiteSpace(playerId))
        {
            return null;
        }

        try
        {
            var blobServiceClient = GetBlobServiceClient();
            var containerClient = blobServiceClient.GetBlobContainerClient(PlayerImageContainer);

            if (!await containerClient.ExistsAsync())
            {
                _logger.LogWarning("Container '{ContainerName}' does not exist.", PlayerImageContainer);
                return null;
            }

            // 1. Direct candidate matching (combining playerId and playerName in various formats)
            var candidates = GenerateBlobNameCandidates(playerName, playerId);

            foreach (var candidate in candidates)
            {
                var blobClient = containerClient.GetBlobClient(candidate);
                if (await blobClient.ExistsAsync())
                {
                    _logger.LogInformation("Found player image at blob '{BlobName}' in container '{ContainerName}'.",
                        candidate, containerClient.Name);

                    var download = await blobClient.DownloadContentAsync();
                    byte[] bytes = download.Value.Content.ToArray();
                    string contentType = DetermineContentType(candidate, download.Value.Details.ContentType, bytes);

                    return (bytes, contentType, candidate);
                }
            }

            // 2. Fast prefix search if playerId is available: e.g. prefix "511933_"
            if (!string.IsNullOrWhiteSpace(playerId))
            {
                string idPrefix = $"{playerId}_";
                string cleanPlayerNorm = !string.IsNullOrWhiteSpace(playerName) ? NormalizePlayerName(playerName) : string.Empty;

                BlobItem? matchedBlob = null;

                await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, idPrefix, default))
                {
                    if (!string.IsNullOrEmpty(cleanPlayerNorm))
                    {
                        string blobWithoutExt = Path.GetFileNameWithoutExtension(blobItem.Name);
                        if (NormalizePlayerName(blobWithoutExt).Contains(cleanPlayerNorm, StringComparison.OrdinalIgnoreCase))
                        {
                            matchedBlob = blobItem;
                            break;
                        }
                    }

                    matchedBlob ??= blobItem;
                }

                if (matchedBlob == null)
                {
                    // Also check for prefix without underscore, e.g. "511933."
                    await foreach (var blobItem in containerClient.GetBlobsAsync(BlobTraits.None, BlobStates.None, $"{playerId}.", default))
                    {
                        matchedBlob = blobItem;
                        break;
                    }
                }

                if (matchedBlob != null)
                {
                    var blobClient = containerClient.GetBlobClient(matchedBlob.Name);
                    var download = await blobClient.DownloadContentAsync();
                    byte[] bytes = download.Value.Content.ToArray();
                    string contentType = DetermineContentType(matchedBlob.Name, download.Value.Details.ContentType, bytes);

                    _logger.LogInformation("Found player image by ID prefix '{Prefix}' at blob '{BlobName}'.",
                        idPrefix, matchedBlob.Name);

                    return (bytes, contentType, matchedBlob.Name);
                }
            }

            // 3. Fuzzy search by playerName across container blobs (e.g. blob name contains '_Stephane Henchoz')
            if (!string.IsNullOrWhiteSpace(playerName))
            {
                string cleanNorm = NormalizePlayerName(playerName);
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    string blobWithoutExt = Path.GetFileNameWithoutExtension(blobItem.Name);
                    int underscoreIdx = blobWithoutExt.IndexOf('_');
                    string playerPart = underscoreIdx >= 0 ? blobWithoutExt[(underscoreIdx + 1)..] : blobWithoutExt;

                    if (NormalizePlayerName(playerPart).Equals(cleanNorm, StringComparison.OrdinalIgnoreCase)
                        || NormalizePlayerName(blobWithoutExt).Contains(cleanNorm, StringComparison.OrdinalIgnoreCase))
                    {
                        var blobClient = containerClient.GetBlobClient(blobItem.Name);
                        var download = await blobClient.DownloadContentAsync();
                        byte[] bytes = download.Value.Content.ToArray();
                        string contentType = DetermineContentType(blobItem.Name, download.Value.Details.ContentType, bytes);

                        _logger.LogInformation("Found player image via name match at blob '{BlobName}'.",
                            blobItem.Name);

                        return (bytes, contentType, blobItem.Name);
                    }
                }
            }

            _logger.LogInformation("Player image for ID: '{PlayerId}', Name: '{PlayerName}' was not found in container '{ContainerName}'.",
                playerId ?? "N/A", playerName ?? "N/A", containerClient.Name);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving player image for ID: '{PlayerId}', Name: '{PlayerName}' from container '{ContainerName}'.",
                playerId ?? "N/A", playerName ?? "N/A", PlayerImageContainer);
            throw;
        }
    }

    private static string NormalizePlayerName(string name)
    {
        return new string(name.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();
    }

    private static List<string> GenerateBlobNameCandidates(string? playerName, string? playerId)
    {
        var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] extensions = [".jpg", ".png", ".jpeg", ".webp", ".svg", ".gif", ""];

        bool hasId = !string.IsNullOrWhiteSpace(playerId);
        bool hasName = !string.IsNullOrWhiteSpace(playerName);

        if (hasId && hasName)
        {
            string id = playerId!.Trim();
            string name = playerName!.Trim();
            string nameLower = name.ToLowerInvariant();
            string nameSlug = nameLower.Replace(" ", "-");
            string nameSnake = nameLower.Replace(" ", "_");
            string nameSquashed = nameLower.Replace(" ", "").Replace("-", "").Replace("_", "");

            bool hasExt = Path.HasExtension(name);
            if (hasExt)
            {
                candidates.Add($"{id}_{name}");
                candidates.Add($"{id}_{nameLower}");
                candidates.Add($"{id}_{nameSlug}");
                candidates.Add($"{id}_{nameSnake}");
                candidates.Add($"{id}_{nameSquashed}");
                candidates.Add($"{id}-{name}");
                candidates.Add(name);
            }

            var nameVariations = new List<string> { name, nameLower, nameSlug, nameSnake, nameSquashed };

            foreach (var nv in nameVariations)
            {
                foreach (var ext in extensions)
                {
                    candidates.Add($"{id}_{nv}{ext}");
                    candidates.Add($"{id}-{nv}{ext}");
                    candidates.Add($"{id}_{nv}");
                }
            }
        }
        else if (hasId && !hasName)
        {
            string id = playerId!.Trim();
            foreach (var ext in extensions)
            {
                candidates.Add($"{id}{ext}");
            }
        }
        else if (hasName && !hasId)
        {
            string trimmed = playerName!.Trim();
            string lower = trimmed.ToLowerInvariant();
            string slug = lower.Replace(" ", "-");
            string snake = lower.Replace(" ", "_");
            string squashed = lower.Replace(" ", "").Replace("-", "").Replace("_", "");

            bool hasExt = Path.HasExtension(trimmed);
            if (hasExt)
            {
                candidates.Add(trimmed);
                candidates.Add(lower);
                candidates.Add(slug);
                candidates.Add(snake);
                candidates.Add(squashed);
            }
            else
            {
                var baseNames = new List<string> { trimmed, lower, slug, snake, squashed };
                var parts = trimmed.Split([' ', '-'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    string lastName = parts[^1].ToLowerInvariant();
                    baseNames.Add(lastName);
                }

                foreach (var baseName in baseNames)
                {
                    foreach (var ext in extensions)
                    {
                        candidates.Add($"{baseName}{ext}");
                    }
                }
            }
        }

        return [.. candidates];
    }

    private static string DetermineContentType(string blobName, string? blobContentType, byte[] bytes)
    {
        if (!string.IsNullOrWhiteSpace(blobContentType)
            && !blobContentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return blobContentType;
        }

        string ext = Path.GetExtension(blobName).ToLowerInvariant();
        switch (ext)
        {
            case ".png":
                return "image/png";
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".webp":
                return "image/webp";
            case ".svg":
                return "image/svg+xml";
            case ".gif":
                return "image/gif";
        }

        // Magic bytes detection
        if (bytes.Length >= 8)
        {
            // PNG: 89 50 4E 47 0D 0A 1A 0A
            if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
                return "image/png";

            // JPEG: FF D8 FF
            if (bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
                return "image/jpeg";

            // GIF: 47 49 46 38
            if (bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38)
                return "image/gif";

            // WebP: RIFF....WEBP
            if (bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46 &&
                bytes.Length >= 12 && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50)
                return "image/webp";
        }

        return "image/jpeg";
    }
}
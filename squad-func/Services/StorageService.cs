
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

    /// <summary>
    /// Uploads data to Azure Storage.
    /// </summary>
    /// <param name="jsonData">The data to upload.</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="containerName">The name of the container to upload to.</param>
    public async Task UploadToStorage(string jsonData, string fileName, string containerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            _logger.LogInformation("Uploading data to {FileName} in Azure Storage container '{ContainerName}'...", fileName, containerName);

            var content = BinaryData.FromString(jsonData);
            await blobClient.UploadAsync(content, overwrite: true);

            _logger.LogInformation("Successfully uploaded {FileName} to Azure Storage.", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading {FileName} to Azure Storage.", fileName);
            _logger.LogError($"Nested error {ex.InnerException?.Message}");
            throw;
        }
    }

    public async Task<int> GetContainerBlobCount(string containerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobs = containerClient.GetBlobsAsync();
            int count = 0;
            await foreach (var blob in blobs)
            {
                count++;
            }

            _logger.LogInformation("Successfully retrieved {Count} blobs from Azure Storage container '{ContainerName}'.", count, containerName);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blobs from Azure Storage container '{ContainerName}'.", containerName);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
        }
    }

    /// <summary>
    /// Gets a list of blobs from Azure Storage.
    /// </summary>
    /// <param name="containerName">The name of the container to get blobs from.</param>
    /// <returns>A list of blob names.</returns>
    public async Task<List<string>> GetBlobs(string containerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobs = containerClient.GetBlobsAsync();
            var blobList = new List<string>();
            await foreach (var blob in blobs)
            {
                blobList.Add(blob.Name);
            }

            _logger.LogInformation("Successfully retrieved {Count} blobs from Azure Storage container '{ContainerName}'.", blobList.Count, containerName);
            return blobList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blobs from Azure Storage container '{ContainerName}'.", containerName);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
        }
    }

    /// <summary>
    /// Checks if a container in Azure Storage is empty.
    /// </summary>
    /// <param name="containerName">The name of the container to check.</param>
    /// <returns>true if the container is empty, false otherwise.</returns>
    public async Task<bool> IsContainerEmpty(string containerName)
    {
        try
        {
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobs = containerClient.GetBlobsAsync();
            if (await blobs.AnyAsync())
            {
                _logger.LogInformation("Container '{ContainerName}' is not empty.", containerName);
                return false;
            }
            // No blobs found, container is empty
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving blobs from Azure Storage container '{ContainerName}'.", containerName);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
        }
    }

    /// <summary>
    /// Reads a blob from Azure Storage.
    /// </summary>
    /// <param name="fileName">The name of the blob to read.</param>
    /// <param name="containerName">The name of the container to read from.</param>
    /// <returns>The content of the blob as a string.</returns>
    public async Task<string?> ReadFromStorage(string fileName, string containerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);
            var content = await blobClient.DownloadContentAsync();

            _logger.LogInformation("Successfully read {FileName} from Azure Storage.", fileName);
            return content.Value.Content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading {FileName} from Azure Storage.", fileName);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
        }
    }

    /// <summary>
    /// Moves a blob from one container to another in Azure Storage.
    /// </summary>
    /// <param name="blob">The name of the blob to move.</param>
    /// <param name="sourceContainerName">The name of the container to move the blob from.</param>
    /// <param name="destinationContainerName">The name of the container to move the blob to.</param>   
    public async Task MoveBlob(string blob, string sourceContainerName, string destinationContainerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["FixtureStorage"]
                ?? Environment.GetEnvironmentVariable("FixtureStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'FixtureStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var sourceContainerClient = blobServiceClient.GetBlobContainerClient(sourceContainerName);
            var destinationContainerClient = blobServiceClient.GetBlobContainerClient(destinationContainerName);

            // Ensure the container exists
            await sourceContainerClient.CreateIfNotExistsAsync();
            await destinationContainerClient.CreateIfNotExistsAsync();

            var blobClient = sourceContainerClient.GetBlobClient(blob);
            var destinationBlobClient = destinationContainerClient.GetBlobClient(blob);

            var content = await blobClient.DownloadContentAsync();
            await destinationBlobClient.UploadAsync(content.Value.Content, overwrite: true);
            await blobClient.DeleteAsync();

            _logger.LogInformation("Successfully moved {Blob} from '{SourceContainerName}' to '{DestinationContainerName}'.", blob, sourceContainerName, destinationContainerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving {Blob} from '{SourceContainerName}' to '{DestinationContainerName}'.", blob, sourceContainerName, destinationContainerName);
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner exception: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
        }
    }

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
    /// <param name="containerName">The target container name (defaults to configured PlayerImageContainer or 'playersname').</param>
    /// <returns>A tuple of the image bytes, MIME content type, and the resolved blob name, or null if not found.</returns>
    public async Task<(byte[] Content, string ContentType, string BlobName)?> GetPlayerImageAsync(
        string? playerName,
        string? playerId = null,
        string? containerName = null)
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

        string targetContainer = !string.IsNullOrWhiteSpace(containerName)
            ? containerName
            : (_configuration["PlayerImageContainer"]
               ?? Environment.GetEnvironmentVariable("PlayerImageContainer")
               ?? "playersname");

        try
        {
            var blobServiceClient = GetBlobServiceClient();
            var containerClient = blobServiceClient.GetBlobContainerClient(targetContainer);

            bool containerExists = await containerClient.ExistsAsync();
            if (!containerExists)
            {
                // Fallback check if container itself is named in playersname format
                string fallbackContainer = !string.IsNullOrWhiteSpace(playerName)
                    ? NormalizePlayerName(playerName)
                    : (playerId ?? "playersname");

                if (!string.Equals(targetContainer, fallbackContainer, StringComparison.OrdinalIgnoreCase))
                {
                    var altContainerClient = blobServiceClient.GetBlobContainerClient(fallbackContainer);
                    if (await altContainerClient.ExistsAsync())
                    {
                        containerClient = altContainerClient;
                        containerExists = true;
                    }
                }

                if (!containerExists)
                {
                    _logger.LogWarning("Container '{ContainerName}' does not exist.", targetContainer);
                    return null;
                }
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
                playerId ?? "N/A", playerName ?? "N/A", targetContainer);
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
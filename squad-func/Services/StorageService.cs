
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace squad_func.Services;

public class StorageService
{
    private readonly ILogger<StorageService> _logger;
    private readonly IConfiguration _configuration;

    public StorageService(ILogger<StorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

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
            _logger.LogError($"Nested error {ex.InnerException?.Message}");
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

            _logger.LogInformation("Reading data from {FileName} in Azure Storage container '{ContainerName}'...", fileName, containerName);

            var content = await blobClient.DownloadContentAsync();

            _logger.LogInformation("Successfully read {FileName} from Azure Storage.", fileName);
            return content.Value.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading {FileName} from Azure Storage.", fileName);
            _logger.LogError($"Nested error {ex.InnerException?.Message}");
            throw;
        }
    }
}

using Azure.Storage.Blobs;
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
            var blobList = new List<string>();
            await foreach (var blob in blobs)
            {
                blobList.Add(blob.Name);
            }

            //_logger.LogInformation("Successfully retrieved {Count} blobs from Azure Storage container '{ContainerName}'.", blobList.Count, containerName);
            if (blobList.Count > 0)
            {
                // container is not empty, we have work to do
                return false;
            }
            // container is empty, nothing to do
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

            _logger.LogInformation("Moving data from {Blob} in Azure Storage container '{SourceContainerName}' to '{DestinationContainerName}'...", blob, sourceContainerName, destinationContainerName);

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
}
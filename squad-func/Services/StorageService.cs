
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

    public async Task UploadToStorage(string filePath, string fileName, string containerName)
    {
        try
        {
            // Fallback to reading from Environment if standard Config value is empty (common in Azure Functions tests)
            string? connectionString = _configuration["AzureWebJobsStorage"]
                ?? Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Storage connection string is missing. Please set 'AzureWebJobsStorage'.");
                throw new InvalidOperationException("Storage connection string is not configured.");
            }

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure the container exists
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(fileName);

            _logger.LogInformation("Uploading file {FileName} to Azure Storage container '{ContainerName}'...", fileName, containerName);

            using var fileStream = File.OpenRead(filePath);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            _logger.LogInformation("Successfully uploaded {FileName} to Azure Storage.", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading {FileName} to Azure Storage.", fileName);
            throw;
        }
    }
}
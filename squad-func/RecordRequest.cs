using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using squad_func.Models;
using squad_func.Services;

namespace Squad.Function;

public class RecordRequest(ILoggerFactory loggerFactory, SquadContext context, IpRateLimiterService rateLimiter)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<RecordRequest>();
    private readonly SquadContext _context = context;
    private readonly IpRateLimiterService _rateLimiter = rateLimiter;

    public class RequestBodyDto
    {
        public DateTime? DateTime { get; set; }
        public string? IpAddress { get; set; }
        public string? Device { get; set; }
    }

    [Function("RecordRequest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "record")] HttpRequest req)
    {
        _logger.LogInformation("Processing HTTP POST request for RecordRequest.");

        // Rate-limit by the server-derived IP (never the client-asserted body value) to
        // stop this anonymous, unauthenticated endpoint from being spammed.
        string forwardedFor = req.Headers["X-Forwarded-For"].ToString();
        string rateLimitKey = !string.IsNullOrEmpty(forwardedFor)
            ? forwardedFor
            : req.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        if (!_rateLimiter.IsAllowed(rateLimitKey))
        {
            _logger.LogWarning("Rate limit exceeded for {Key} on RecordRequest.", rateLimitKey);
            return new ObjectResult(new { error = "Too many requests." }) { StatusCode = StatusCodes.Status429TooManyRequests };
        }

        try
        {
            RequestBodyDto? data;
            
            // Read and deserialize the request body
            using (var reader = new StreamReader(req.Body))
            {
                var bodyStr = await reader.ReadToEndAsync();
                if (string.IsNullOrWhiteSpace(bodyStr))
                {
                    return new BadRequestObjectResult(new { error = "Request body is empty." });
                }

                data = JsonSerializer.Deserialize<RequestBodyDto>(bodyStr, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            if (data == null)
            {
                return new BadRequestObjectResult(new { error = "Failed to parse JSON body." });
            }

            // Extract values, utilizing fallback values from headers/context if fields are missing in body
            DateTime recordedTime = data.DateTime ?? DateTime.UtcNow;
            
            string ipAddress = data?.IpAddress?.ToString() 
                ?? (string)req.Headers["X-Forwarded-For"] // Implicit cast to string returns null if missing
                ?? req.HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "Unknown";
            
            data.IpAddress = ipAddress; // Update the data object with the resolved IP address

            string device = data.Device 
                ?? req.Headers["User-Agent"].ToString() 
                ?? "Unknown";

            // Log the request details
            _logger.LogInformation("Logged Event - Time: {Time}, IP: {IP}, Device: {Device}", 
                recordedTime, ipAddress, device);

            // Create and save database Event log
            string eventMessage = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var newEvent = new Event
            {
                Title = "Access",
                Message = eventMessage,
                Level = "Info",
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            // Return success with the processed data
            return new OkObjectResult(new
            {
                message = "Request successfully logged.",
                dateTime = recordedTime,
                ipAddress = ipAddress,
                device = device
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON Deserialization failed.");
            return new BadRequestObjectResult(new { error = "Invalid JSON format.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

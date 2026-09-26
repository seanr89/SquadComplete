using System;
using System.IO;
using System.Linq;
using System.Net;
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

    private const int MaxDeviceLength = 512;

    // Only the device hint is accepted from the client. IP address and timestamp are always
    // derived server-side so the access log can't be forged; any such fields in the body are ignored.
    public class RequestBodyDto
    {
        public string? Device { get; set; }
    }

    // Shape persisted to Event.Message (same field names as the previous client-supplied payload).
    private record AccessRecord(DateTime DateTime, string IpAddress, string Device);

    [Function("RecordRequest")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "record")] HttpRequest req)
    {
        _logger.LogInformation("Processing HTTP POST request for RecordRequest.");

        // Rate-limit by the server-derived IP (never the client-asserted body value) to
        // stop this anonymous, unauthenticated endpoint from being spammed.
        string ipAddress = ResolveClientIp(req);

        if (!_rateLimiter.IsAllowed(ipAddress))
        {
            _logger.LogWarning("Rate limit exceeded for {Key} on RecordRequest.", ipAddress);
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

            // Timestamp and IP are server-derived; only the device string comes from the client,
            // preferring the User-Agent header and capping length to keep the log bounded.
            DateTime recordedTime = DateTime.UtcNow;

            string device = req.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(device))
            {
                device = string.IsNullOrWhiteSpace(data.Device) ? "Unknown" : data.Device;
            }
            if (device.Length > MaxDeviceLength)
            {
                device = device[..MaxDeviceLength];
            }

            // Log the request details
            _logger.LogInformation("Logged Event - Time: {Time}, IP: {IP}, Device: {Device}", 
                recordedTime, ipAddress, device);

            // Create and save database Event log
            string eventMessage = JsonSerializer.Serialize(new AccessRecord(recordedTime, ipAddress, device), new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var newEvent = new Event
            {
                Title = "Access",
                Message = eventMessage,
                Level = "Info",
                CreatedAt = recordedTime
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

    /// <summary>
    /// Resolves the caller's IP from what the Azure front end observed, never from the request body.
    /// Azure appends the real client address as the right-most X-Forwarded-For entry (any entries to
    /// its left are client-supplied and untrusted), sometimes with a port, so take the last entry and
    /// strip the port. Falls back to the connection's remote address when the header is absent.
    /// </summary>
    private static string ResolveClientIp(HttpRequest req)
    {
        string? lastForwarded = req.Headers["X-Forwarded-For"].ToString()
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .LastOrDefault();

        if (!string.IsNullOrEmpty(lastForwarded) && IPEndPoint.TryParse(lastForwarded, out var endpoint))
        {
            return endpoint.Address.ToString();
        }

        return req.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

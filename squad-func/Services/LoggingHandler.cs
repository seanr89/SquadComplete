using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace squad_func.Services;

public class LoggingHandler(ILogger<LoggingHandler> logger) : DelegatingHandler
{
    private readonly ILogger<LoggingHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HTTP Request Failed with Status Code: {StatusCode} for {Method} {Uri} in {ElapsedMs}ms",
                    (int)response.StatusCode, request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "HTTP Request Failed with Exception: {Method} {Uri} after {ElapsedMs}ms - Error: {Message}",
                request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds, ex.Message);
            throw;
        }
    }
}

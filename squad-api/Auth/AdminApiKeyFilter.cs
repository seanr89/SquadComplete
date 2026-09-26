using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace squad_api.Auth;

/// <summary>
/// Endpoint filter that restricts admin-only endpoints (data management writes, internal logs)
/// to callers presenting the configured admin API key in the <c>X-Api-Key</c> header.
/// Fails closed: if no key is configured (<c>Auth:AdminApiKey</c>), every request is rejected.
/// </summary>
public class AdminApiKeyFilter(IConfiguration configuration, ILogger<AdminApiKeyFilter> logger) : IEndpointFilter
{
    public const string HeaderName = "X-Api-Key";
    public const string ConfigKey = "Auth:AdminApiKey";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = configuration[ConfigKey];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            logger.LogWarning("Admin endpoint {Path} called but {ConfigKey} is not configured; rejecting.",
                context.HttpContext.Request.Path, ConfigKey);
            return Results.Unauthorized();
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || !KeysMatch(providedKey.ToString(), configuredKey))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }

    // Constant-time comparison so the key can't be recovered via response timing.
    private static bool KeysMatch(string provided, string expected) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(provided), Encoding.UTF8.GetBytes(expected));
}

public static class AdminApiKeyExtensions
{
    /// <summary>Requires a valid admin API key (<c>X-Api-Key</c> header) to call this endpoint.</summary>
    public static RouteHandlerBuilder RequireAdminApiKey(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<AdminApiKeyFilter>();
}

using System.Collections.Concurrent;

namespace squad_func.Services;

/// <summary>
/// Simple in-memory fixed-window rate limiter for anonymous HTTP-triggered functions.
/// Azure Functions has no built-in per-endpoint rate limiting, so this guards public
/// write endpoints (e.g. RecordRequest) from being spammed within a single instance.
/// Not a substitute for edge/WAF-level throttling under multi-instance scale-out.
/// </summary>
public class IpRateLimiterService
{
    private readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _windows = new();
    private readonly int _permitLimit;
    private readonly TimeSpan _window;

    public IpRateLimiterService(int permitLimit = 10, TimeSpan? window = null)
    {
        _permitLimit = permitLimit;
        _window = window ?? TimeSpan.FromMinutes(1);
    }

    public bool IsAllowed(string key)
    {
        var now = DateTime.UtcNow;

        var entry = _windows.AddOrUpdate(
            key,
            _ => (1, now),
            (_, existing) =>
            {
                if (now - existing.WindowStart > _window)
                {
                    return (1, now);
                }
                return (existing.Count + 1, existing.WindowStart);
            });

        return entry.Count <= _permitLimit;
    }
}

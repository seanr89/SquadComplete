using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using squad_func.Models;

namespace squad_func.Services;

public interface IApiService
{
    Task<List<PlayerStatsResponse>?> GetPlayerStatsAsync(int fixtureId, int teamId);
    Task<FixtureApiResponse?> GetFixtureDataAsync(int fixtureId);
    Task<ApiTeamDetail?> GetTeamDataAsync(int teamId);
    Task<PlayerStatsPlayerInfo?> GetPlayerProfileAsync(string search);
    Task GetFixturesForLeague(int leagueid, DateTime date);
    Task<string> GetPlayerByNameAsync(string? name);
    Task<string> GetTeamByNameAsync(string? name);
    Task<string> GetLeagueByNameAsync(string name);
}

public class ApiService(HttpClient httpClient, ILogger<ApiService> logger) : IApiService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly string ApiKey = Environment.GetEnvironmentVariable("FootballApiKey") ?? throw new ArgumentNullException("FootballApiKey");
    private readonly string BaseUrl = Environment.GetEnvironmentVariable("FootballBaseUrl") ?? throw new ArgumentNullException("FootballBaseUrl");

    /// <summary>
    /// Fetches fixtures for a specific league and date.
    /// </summary>
    /// <param name="leagueid">The ID of the league.</param>
    /// <param name="date">The date for which to fetch fixtures.</param>
    public async Task GetFixturesForLeague(int leagueid, DateTime date)
    {
        var season = date.Year;
        var url = $"{BaseUrl}/fixtures?league={leagueid}&season={season}&date={date}&status=ft";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", BaseUrl);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Response was not successful for league {LeagueId} and date {Date}. Status code: {StatusCode}", leagueid, date, response.StatusCode);
                throw new HttpRequestException($"HTTP error! status: {(int)response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(content);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data from {Url}", url);
            throw;
        }
    }

    public async Task<FixtureApiResponse?> GetFixtureDataAsync(int fixtureId)
    {
        var url = $"{BaseUrl}/fixtures?id={fixtureId}";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", BaseUrl);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("response", out var responseData) && responseData.ValueKind == JsonValueKind.Array && responseData.GetArrayLength() > 0)
            {
                return JsonSerializer.Deserialize<FixtureApiResponse>(responseData[0].GetRawText());
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data from {Url}", url);
            throw;
        }
    }

    public async Task<ApiTeamDetail?> GetTeamDataAsync(int teamId)
    {
        var url = $"{BaseUrl}/teams?id={teamId}";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", BaseUrl);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("response", out var responseData) && responseData.ValueKind == JsonValueKind.Array && responseData.GetArrayLength() > 0)
            {
                var teamWrapper = JsonSerializer.Deserialize<TeamApiResponseWrapper>(responseData[0].GetRawText());
                return teamWrapper?.Team;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching team data from {Url}", url);
            return null;
        }
    }

    #region Football Data API - AI Search

    /// <summary>
    /// Gets player information by name or other criteria.
    /// </summary>
    public async Task<string> GetPlayerByNameAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        var searchName = name.Trim();
        var nameParts = searchName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (nameParts.Length > 0)
        {
            searchName = nameParts.Last();
        }

        var encodedName = Uri.EscapeDataString(searchName);
        var requestUrl = $"{BaseUrl}/players/profiles?search={encodedName}";
        _logger.LogInformation($"Getting player by name for url: {requestUrl}");
        Thread.Sleep(2500);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("x-rapidapi-key", ApiKey);
        request.Headers.Add("x-rapidapi-host", BaseUrl);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets team information by team name.
    /// Example: "manchester united"
    /// </summary>
    public async Task<string> GetTeamByNameAsync(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;
        var encodedName = Uri.EscapeDataString(name);
        var requestUrl = $"{BaseUrl}/teams?name={encodedName}";
        _logger.LogInformation("Getting team by name for url: {RequestUrl}", requestUrl);
        Thread.Sleep(2500);

        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("x-rapidapi-key", ApiKey);
        request.Headers.Add("x-rapidapi-host", BaseUrl);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Gets league information by league name.
    /// Example: "premier league"
    /// </summary>
    public async Task<string> GetLeagueByNameAsync(string name)
    {
        var encodedName = Uri.EscapeDataString(name);
        var requestUrl = $"{BaseUrl}/leagues?name={encodedName}";
        using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
        request.Headers.Add("x-rapidapi-key", ApiKey);
        request.Headers.Add("x-rapidapi-host", BaseUrl);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    #endregion

    public async Task<PlayerStatsPlayerInfo?> GetPlayerProfileAsync(string search)
    {
        var url = $"{BaseUrl}/players/profiles?search={Uri.EscapeDataString(search)}";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", BaseUrl);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("response", out var responseData) && responseData.ValueKind == JsonValueKind.Array && responseData.GetArrayLength() > 0)
            {
                var playerWrapper = JsonSerializer.Deserialize<PlayerProfileApiResponseWrapper>(responseData[0].GetRawText());
                return playerWrapper?.Player;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching player profile from {Url}", url);
            return null;
        }
    }

    public async Task<List<PlayerStatsResponse>?> GetPlayerStatsAsync(int fixtureId, int teamId)
    {
        var url = $"{BaseUrl}/fixtures/players?fixture={fixtureId}&team={teamId}";
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("x-rapidapi-key", ApiKey);
            request.Headers.Add("x-rapidapi-host", BaseUrl);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Response was not successful for fixture {FixtureId} and team {TeamId}. Status code: {StatusCode}", fixtureId, teamId, response.StatusCode);
                throw new HttpRequestException($"HTTP error! status: {(int)response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            using var jsonDocument = JsonDocument.Parse(content);

            if (jsonDocument.RootElement.TryGetProperty("response", out var responseData))
            {
                return JsonSerializer.Deserialize<List<PlayerStatsResponse>>(responseData.GetRawText());
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching player stats for fixture {FixtureId}, team {TeamId}: {Message}", fixtureId, teamId, ex.Message);
            return null;
        }
    }
}

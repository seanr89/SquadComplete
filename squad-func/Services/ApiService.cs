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
    // Interface ready for future football data API calls
    Task<string> GetAsync(string url);
    Task<List<PlayerStatsResponse>?> GetPlayerStatsAsync(int fixtureId, int teamId);
    Task<FixtureApiResponse?> GetFixtureDataAsync(int fixtureId);
    Task<ApiTeamDetail?> GetTeamDataAsync(int teamId);
}

public class ApiService(HttpClient httpClient, ILogger<ApiService> logger) : IApiService
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ILogger<ApiService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly string ApiKey = Environment.GetEnvironmentVariable("FootballApiKey") ?? throw new ArgumentNullException("FootballApiKey");
    private readonly string BaseUrl = Environment.GetEnvironmentVariable("FootballBaseUrl") ?? throw new ArgumentNullException("FootballBaseUrl");

    public async Task<string> GetAsync(string url)
    {
        try
        {
            //_logger.LogInformation("Sending GET request to {Url}", url);
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data from {Url}", url);
            throw;
        }
    }

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

            // if (jsonDocument.RootElement.TryGetProperty("response", out var responseData))
            // {
            //     return JsonSerializer.Deserialize<List<PlayerStatsResponse>>(responseData.GetRawText());
            // }

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

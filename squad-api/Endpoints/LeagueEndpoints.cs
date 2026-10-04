using squad_api.DTOs;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class LeagueEndpoints
{
    /// <summary>
    /// Maps the league management endpoints for the API.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapLeagueEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapCrud("/api/leagues", new CrudEndpointConfig<League, LeagueDto>
        {
            Singular = "League",
            Plural = "Leagues",
            Create = dto => new League
            {
                Name = dto.Name,
                Type = dto.Type,
                Logo = dto.Logo,
                CountryName = dto.CountryName,
                CountryCode = dto.CountryCode,
                CountryFlag = dto.CountryFlag,
                ApiId = dto.ApiId
            },
            ApplyUpdate = (league, dto) =>
            {
                league.Name = dto.Name;
                league.Type = dto.Type;
                league.Logo = dto.Logo;
                league.CountryName = dto.CountryName;
                league.CountryCode = dto.CountryCode;
                league.CountryFlag = dto.CountryFlag;
                league.ApiId = dto.ApiId;
                league.UpdatedAt = DateTime.UtcNow;
            }
        });
    }
}

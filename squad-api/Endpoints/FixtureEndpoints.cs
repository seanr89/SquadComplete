using Microsoft.EntityFrameworkCore;
using squad_api.DTOs;

namespace squad_api.Endpoints;

public static class FixtureEndpoints
{
    /// <summary>
    /// Maps the fixture management endpoints for the API. Reads include the associated league.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapFixtureEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapCrud("/api/fixtures", new CrudEndpointConfig<Fixture, FixtureDto, FixtureResponse>
        {
            Singular = "Fixture",
            ToResponse = FixtureResponse.From,
            Plural = "Fixtures",
            Includes = query => query.Include(f => f.League),
            Create = dto => new Fixture
            {
                LeagueId = dto.LeagueId,
                HomeTeamId = dto.HomeTeamId,
                HomeTeamName = dto.HomeTeamName,
                AwayTeamId = dto.AwayTeamId,
                AwayTeamName = dto.AwayTeamName,
                HomeGoalCount = dto.HomeGoalCount,
                AwayGoalCount = dto.AwayGoalCount,
                FixtureDate = dto.FixtureDate,
                FixtureSource = dto.FixtureSource,
                ApiId = dto.ApiId
            },
            ApplyUpdate = (fixture, dto) =>
            {
                fixture.LeagueId = dto.LeagueId;
                fixture.HomeTeamId = dto.HomeTeamId;
                fixture.HomeTeamName = dto.HomeTeamName;
                fixture.AwayTeamId = dto.AwayTeamId;
                fixture.AwayTeamName = dto.AwayTeamName;
                fixture.HomeGoalCount = dto.HomeGoalCount;
                fixture.AwayGoalCount = dto.AwayGoalCount;
                fixture.FixtureDate = dto.FixtureDate;
                fixture.FixtureSource = dto.FixtureSource;
                fixture.ApiId = dto.ApiId;
                fixture.UpdatedAt = DateTime.UtcNow;
            }
        });
    }
}

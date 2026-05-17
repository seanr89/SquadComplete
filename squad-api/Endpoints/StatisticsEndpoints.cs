using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.DTOs;

namespace squad_api.Endpoints;

public static class StatisticsEndpoints
{
    public static void MapStatisticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/statistics").WithTags("Statistics");

        group.MapGet("/", async (SquadContext db) =>
        {
            var stats = new StatisticsDto
            {
                Leagues = await db.Leagues.CountAsync(),
                Teams = await db.Teams.CountAsync(),
                Players = await db.Players.CountAsync(),
                Fixtures = await db.Fixtures.CountAsync(),
                Games = await db.GameRecords.CountAsync()
            };

            return Results.Ok(stats);
        })
        .WithName("GetStatistics");
    }
}

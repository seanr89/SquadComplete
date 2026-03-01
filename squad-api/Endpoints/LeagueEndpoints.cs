using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class LeagueEndpoints
{
    public static void MapLeagueEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/leagues").WithTags(nameof(League));

        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Leagues.ToListAsync();
        })
        .WithName("GetAllLeagues");

        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Leagues.FindAsync(id)
                is League model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetLeagueById");

        group.MapPut("/{id}", async (int id, League inputLeague, SquadContext db) =>
        {
            var foundModel = await db.Leagues.FindAsync(id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.Name = inputLeague.Name;
            foundModel.Type = inputLeague.Type;
            foundModel.Logo = inputLeague.Logo;
            foundModel.CountryName = inputLeague.CountryName;
            foundModel.CountryCode = inputLeague.CountryCode;
            foundModel.CountryFlag = inputLeague.CountryFlag;
            foundModel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateLeague");

        group.MapPost("/", async (League league, SquadContext db) =>
        {
            db.Leagues.Add(league);
            await db.SaveChangesAsync();
            return Results.Created($"/api/leagues/{league.Id}", league);
        })
        .WithName("CreateLeague");

        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.Leagues.FindAsync(id) is League league)
            {
                db.Leagues.Remove(league);
                await db.SaveChangesAsync();
                return Results.Ok(league);
            }

            return Results.NotFound();
        })
        .WithName("DeleteLeague");
    }
}

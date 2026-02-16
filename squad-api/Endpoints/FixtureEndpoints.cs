using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class FixtureEndpoints
{
    public static void MapFixtureEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/fixtures").WithTags(nameof(Fixture));

        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Fixtures.Include(f => f.League).ToListAsync();
        })
        .WithName("GetAllFixtures")
        .WithOpenApi();

        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Fixtures.Include(f => f.League).FirstOrDefaultAsync(f => f.Id == id)
                is Fixture model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetFixtureById")
        .WithOpenApi();

        group.MapPut("/{id}", async (int id, Fixture inputFixture, SquadContext db) =>
        {
            var foundModel = await db.Fixtures.FindAsync(id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.LeagueId = inputFixture.LeagueId;
            foundModel.HomeTeamId = inputFixture.HomeTeamId;
            foundModel.HomeTeamName = inputFixture.HomeTeamName;
            foundModel.AwayTeamId = inputFixture.AwayTeamId;
            foundModel.AwayTeamName = inputFixture.AwayTeamName;
            foundModel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateFixture")
        .WithOpenApi();

        group.MapPost("/", async (Fixture fixture, SquadContext db) =>
        {
            db.Fixtures.Add(fixture);
            await db.SaveChangesAsync();
            return Results.Created($"/api/fixtures/{fixture.Id}", fixture);
        })
        .WithName("CreateFixture")
        .WithOpenApi();

        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.Fixtures.FindAsync(id) is Fixture fixture)
            {
                db.Fixtures.Remove(fixture);
                await db.SaveChangesAsync();
                return Results.Ok(fixture);
            }

            return Results.NotFound();
        })
        .WithName("DeleteFixture")
        .WithOpenApi();
    }
}

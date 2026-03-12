using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class FixtureEndpoints
{
    /// <summary>
    /// Maps the fixture management endpoints for the API.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapFixtureEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/fixtures").WithTags(nameof(Fixture));

        /// <summary>
        /// Retrieves all fixtures along with their associated league information.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <returns>A list of all fixtures.</returns>
        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Fixtures.Include(f => f.League).ToListAsync();
        })
        .WithName("GetAllFixtures");

        /// <summary>
        /// Retrieves a specific fixture by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the fixture.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The requested fixture if found; otherwise, a 404 Not Found response.</returns>
        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Fixtures.Include(f => f.League).FirstOrDefaultAsync(f => f.Id == id)
                is Fixture model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetFixtureById");

        /// <summary>
        /// Updates an existing fixture's details.
        /// </summary>
        /// <param name="id">The unique identifier of the fixture to update.</param>
        /// <param name="inputFixture">The updated fixture data.</param>
        /// <param name="db">The database context.</param>
        /// <returns>A 204 No Content response if successful; otherwise, a 404 Not Found response.</returns>
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
            foundModel.HomeGoalCount = inputFixture.HomeGoalCount;
            foundModel.AwayGoalCount = inputFixture.AwayGoalCount;
            foundModel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateFixture");

        /// <summary>
        /// Creates a new fixture.
        /// </summary>
        /// <param name="fixture">The fixture data to create.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The newly created fixture with a 201 Created response.</returns>
        group.MapPost("/", async (Fixture fixture, SquadContext db) =>
        {
            db.Fixtures.Add(fixture);
            await db.SaveChangesAsync();
            return Results.Created($"/api/fixtures/{fixture.Id}", fixture);
        })
        .WithName("CreateFixture");

        /// <summary>
        /// Deletes a specific fixture by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the fixture to delete.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The deleted fixture if successful; otherwise, a 404 Not Found response.</returns>
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
        .WithName("DeleteFixture");
    }
}

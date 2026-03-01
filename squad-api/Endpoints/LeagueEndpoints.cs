using Microsoft.EntityFrameworkCore;
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
        var group = routes.MapGroup("/api/leagues").WithTags(nameof(League));

        /// <summary>
        /// Retrieves all leagues.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <returns>A list of all leagues.</returns>
        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Leagues.ToListAsync();
        })
        .WithName("GetAllLeagues");

        /// <summary>
        /// Retrieves a specific league by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the league.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The requested league if found; otherwise, a 404 Not Found response.</returns>
        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Leagues.FindAsync(id)
                is League model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetLeagueById");

        /// <summary>
        /// Updates an existing league's details.
        /// </summary>
        /// <param name="id">The unique identifier of the league to update.</param>
        /// <param name="inputLeague">The updated league data.</param>
        /// <param name="db">The database context.</param>
        /// <returns>A 204 No Content response if successful; otherwise, a 404 Not Found response.</returns>
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

        /// <summary>
        /// Creates a new league.
        /// </summary>
        /// <param name="league">The league data to create.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The newly created league with a 201 Created response.</returns>
        group.MapPost("/", async (League league, SquadContext db) =>
        {
            db.Leagues.Add(league);
            await db.SaveChangesAsync();
            return Results.Created($"/api/leagues/{league.Id}", league);
        })
        .WithName("CreateLeague");

        /// <summary>
        /// Deletes a specific league by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the league to delete.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The deleted league if successful; otherwise, a 404 Not Found response.</returns>
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

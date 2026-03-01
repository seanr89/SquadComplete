using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class PlayerEndpoints
{
    /// <summary>
    /// Maps the player management endpoints for the API.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/players").WithTags(nameof(Player));

        /// <summary>
        /// Retrieves all players.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <returns>A list of all players.</returns>
        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Players.ToListAsync();
        })
        .WithName("GetAllPlayers");

        /// <summary>
        /// Retrieves a specific player by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the player.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The requested player if found; otherwise, a 404 Not Found response.</returns>
        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Players.FindAsync(id)
                is Player model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetPlayerById");

        /// <summary>
        /// Updates an existing player's details.
        /// </summary>
        /// <param name="id">The unique identifier of the player to update.</param>
        /// <param name="inputPlayer">The updated player data.</param>
        /// <param name="db">The database context.</param>
        /// <returns>A 204 No Content response if successful; otherwise, a 404 Not Found response.</returns>
        group.MapPut("/{id}", async (int id, Player inputPlayer, SquadContext db) =>
        {
            var foundModel = await db.Players.FindAsync(id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.Name = inputPlayer.Name;
            foundModel.Photo = inputPlayer.Photo;
            foundModel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdatePlayer");

        /// <summary>
        /// Creates a new player.
        /// </summary>
        /// <param name="player">The player data to create.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The newly created player with a 201 Created response.</returns>
        group.MapPost("/", async (Player player, SquadContext db) =>
        {
            db.Players.Add(player);
            await db.SaveChangesAsync();
            return Results.Created($"/api/players/{player.Id}", player);
        })
        .WithName("CreatePlayer");

        /// <summary>
        /// Deletes a specific player by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the player to delete.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The deleted player if successful; otherwise, a 404 Not Found response.</returns>
        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.Players.FindAsync(id) is Player player)
            {
                db.Players.Remove(player);
                await db.SaveChangesAsync();
                return Results.Ok(player);
            }

            return Results.NotFound();
        })
        .WithName("DeletePlayer");
    }
}

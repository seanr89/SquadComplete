using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class PlayerEndpoints
{
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/players").WithTags(nameof(Player));

        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Players.ToListAsync();
        })
        .WithName("GetAllPlayers");

        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Players.FindAsync(id)
                is Player model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetPlayerById");

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

        group.MapPost("/", async (Player player, SquadContext db) =>
        {
            db.Players.Add(player);
            await db.SaveChangesAsync();
            return Results.Created($"/api/players/{player.Id}", player);
        })
        .WithName("CreatePlayer");

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

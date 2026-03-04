using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class PlayerFixtureStatisticEndpoints
{
    public static void MapPlayerFixtureStatisticEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/player-fixture-statistics").WithTags("PlayerFixtureStatistic");

        /// <summary>
        /// Retrieves all player fixture statistics, including related Fixture, Player, and Team data.
        /// </summary>
        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.PlayerFixtureStatistics
                .Include(pfs => pfs.Fixture)
                .Include(pfs => pfs.Player)
                .Include(pfs => pfs.Team)
                .ToListAsync();
        })
        .WithName("GetAllPlayerFixtureStatistics");

        /// <summary>
        /// Retrieves a specific player fixture statistic by fixture ID and player ID.
        /// </summary>
        group.MapGet("/{fixtureId}/{playerId}", async (int fixtureId, int playerId, SquadContext db) =>
        {
            return await db.PlayerFixtureStatistics
                .Include(pfs => pfs.Fixture)
                .Include(pfs => pfs.Player)
                .Include(pfs => pfs.Team)
                .FirstOrDefaultAsync(pfs => pfs.FixtureId == fixtureId && pfs.PlayerId == playerId)
                is PlayerFixtureStatistic model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetPlayerFixtureStatisticById");

        /// <summary>
        /// Updates an existing player fixture statistic.
        /// </summary>
        group.MapPut("/{fixtureId}/{playerId}", async (int fixtureId, int playerId, PlayerFixtureStatistic inputPfs, SquadContext db) =>
        {
            var foundModel = await db.PlayerFixtureStatistics.FindAsync(fixtureId, playerId);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.TeamId = inputPfs.TeamId;
            foundModel.Minutes = inputPfs.Minutes;
            foundModel.Number = inputPfs.Number;
            foundModel.Position = inputPfs.Position;
            foundModel.Rating = inputPfs.Rating;
            foundModel.IsCaptain = inputPfs.IsCaptain;
            foundModel.IsSubstitute = inputPfs.IsSubstitute;
            foundModel.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdatePlayerFixtureStatistic");

        /// <summary>
        /// Creates a new player fixture statistic.
        /// </summary>
        group.MapPost("/", async (PlayerFixtureStatistic pfs, SquadContext db) =>
        {
            db.PlayerFixtureStatistics.Add(pfs);
            await db.SaveChangesAsync();
            return Results.Created($"/api/player-fixture-statistics/{pfs.FixtureId}/{pfs.PlayerId}", pfs);
        })
        .WithName("CreatePlayerFixtureStatistic");

        /// <summary>
        /// Deletes a specific player fixture statistic by fixture ID and player ID.
        /// </summary>
        group.MapDelete("/{fixtureId}/{playerId}", async (int fixtureId, int playerId, SquadContext db) =>
        {
            if (await db.PlayerFixtureStatistics.FindAsync(fixtureId, playerId) is PlayerFixtureStatistic pfs)
            {
                db.PlayerFixtureStatistics.Remove(pfs);
                await db.SaveChangesAsync();
                return Results.Ok(pfs);
            }

            return Results.NotFound();
        })
        .WithName("DeletePlayerFixtureStatistic");
    }
}

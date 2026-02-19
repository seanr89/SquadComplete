using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class GameRecordEndpoints
{
    public static void MapGameRecordEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/game-records").WithTags(nameof(GameRecord));

        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.GameRecords
                .Include(gr => gr.Tags)
                .ToListAsync();
        })
        .WithName("GetAllGameRecords")
        .WithOpenApi();

        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.GameRecords
                .Include(gr => gr.Tags)
                .FirstOrDefaultAsync(gr => gr.Id == id)
                is GameRecord model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetGameRecordById")
        .WithOpenApi();

        group.MapPut("/{id}", async (int id, GameRecord inputRecord, SquadContext db) =>
        {
            var foundModel = await db.GameRecords
                .Include(gr => gr.Tags)
                .FirstOrDefaultAsync(gr => gr.Id == id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.GameDate = inputRecord.GameDate;
            foundModel.UpdatedAt = DateTime.UtcNow;

            // Handle Tags Update
            if (inputRecord.Tags != null && inputRecord.Tags.Any())
            {
                db.GameRecordTags.RemoveRange(foundModel.Tags);
                foundModel.Tags.Clear();
                foreach (var tag in inputRecord.Tags)
                {
                    tag.GameRecordId = foundModel.Id; // Ensure FK is correct
                    foundModel.Tags.Add(tag);
                }
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateGameRecord")
        .WithOpenApi();

        group.MapPost("/", async (GameRecord record, SquadContext db) =>
        {
            // Reset dates on creation
            record.CreatedAt = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            db.GameRecords.Add(record);
            await db.SaveChangesAsync();
            return Results.Created($"/api/game-records/{record.Id}", record);
        })
        .WithName("CreateGameRecord")
        .WithOpenApi();

        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.GameRecords.Include(gr => gr.Tags).FirstOrDefaultAsync(gr => gr.Id == id) is GameRecord record)
            {
                db.GameRecords.Remove(record);
                await db.SaveChangesAsync();
                return Results.Ok(record);
            }

            return Results.NotFound();
        })
        .WithName("DeleteGameRecord")
        .WithOpenApi();
    }
}

using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.Services;

namespace squad_api.Endpoints;

public static class GameRecordEndpoints
{
    public static void MapGameRecordEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/game-records").WithTags(nameof(GameRecord));

        group.MapGet("/{id:int}", async (int id, GameRecordService service) =>
        {
            var recordDto = await service.GetGameRecordByIdAsync(id);
            return recordDto != null 
                ? Results.Ok(recordDto) 
                : Results.NotFound();
        })
        .WithName("GetGameRecordById");
        
        //get game record by date
        group.MapGet("/date/{date}", async (DateTime date, GameRecordService service) =>
        {
            var recordDto = await service.GetGameRecordByDateAsync(date);
            return recordDto != null 
                ? Results.Ok(recordDto) 
                : Results.NotFound();
        })
        .WithName("GetGameRecordByDate");

        group.MapPut("/{id:int}", async (int id, GameRecord inputRecord, SquadContext db) =>
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
        .WithName("UpdateGameRecord");

        group.MapPost("/", async (GameRecord record, GameRecordService service) =>
        {
            var createdRecordDto = await service.CreateGameRecordAsync(record);
            return Results.Created($"/api/game-records/{createdRecordDto.Id}", createdRecordDto);
        })
        .WithName("CreateGameRecord");

        group.MapDelete("/{id:int}", async (int id, SquadContext db) =>
        {
            if (await db.GameRecords.Include(gr => gr.Tags).FirstOrDefaultAsync(gr => gr.Id == id) is GameRecord record)
            {
                db.GameRecords.Remove(record);
                await db.SaveChangesAsync();
                return Results.Ok(record);
            }

            return Results.NotFound();
        })
        .WithName("DeleteGameRecord");
    }
}

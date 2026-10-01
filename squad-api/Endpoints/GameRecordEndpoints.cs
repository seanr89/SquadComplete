using Microsoft.EntityFrameworkCore;
using squad_api.Models;
using squad_api.Services;
using squad_api.Auth;

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

            if (inputRecord.FormationId.HasValue &&
                !await db.Formations.AnyAsync(f => f.Id == inputRecord.FormationId.Value))
            {
                return Results.BadRequest($"Formation {inputRecord.FormationId} does not exist.");
            }

            var incomingTags = (inputRecord.Tags ?? new List<GameRecordTag>())
                .Select(t => (t.FixtureId, t.TeamId))
                .Distinct()
                .ToList();

            if (incomingTags.Count > 0)
            {
                var fixtureIds = incomingTags.Select(t => t.FixtureId).Distinct().ToList();
                var teamIds = incomingTags.Select(t => t.TeamId).Distinct().ToList();

                var existingFixtureIds = await db.Fixtures
                    .AsNoTracking()
                    .Where(f => fixtureIds.Contains(f.Id))
                    .Select(f => f.Id)
                    .ToListAsync();
                var missingFixtures = fixtureIds.Except(existingFixtureIds).ToList();
                if (missingFixtures.Count > 0)
                {
                    return Results.BadRequest($"Fixture(s) not found: {string.Join(", ", missingFixtures)}.");
                }

                var existingTeamIds = await db.Teams
                    .AsNoTracking()
                    .Where(t => teamIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .ToListAsync();
                var missingTeams = teamIds.Except(existingTeamIds).ToList();
                if (missingTeams.Count > 0)
                {
                    return Results.BadRequest($"Team(s) not found: {string.Join(", ", missingTeams)}.");
                }
            }

            foundModel.GameDate = inputRecord.GameDate;
            foundModel.FormationId = inputRecord.FormationId;
            foundModel.UpdatedAt = DateTime.UtcNow;

            // Only touch tags when supplied; diff so unchanged tags are left alone
            if (incomingTags.Count > 0)
            {
                var incomingSet = incomingTags.ToHashSet();
                var toRemove = foundModel.Tags
                    .Where(t => !incomingSet.Contains((t.FixtureId, t.TeamId)))
                    .ToList();
                db.GameRecordTags.RemoveRange(toRemove);

                var existingSet = foundModel.Tags.Select(t => (t.FixtureId, t.TeamId)).ToHashSet();
                foreach (var (fixtureId, teamId) in incomingSet.Where(t => !existingSet.Contains(t)))
                {
                    foundModel.Tags.Add(new GameRecordTag
                    {
                        GameRecordId = foundModel.Id,
                        FixtureId = fixtureId,
                        TeamId = teamId
                    });
                }
            }

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateGameRecord")
        .RequireAdminApiKey();

        group.MapPost("/", async (GameRecord record, GameRecordService service) =>
        {
            var createdRecordDto = await service.CreateGameRecordAsync(record);
            return Results.Created($"/api/game-records/{createdRecordDto.Id}", createdRecordDto);
        })
        .WithName("CreateGameRecord")
        .RequireAdminApiKey();

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
        .WithName("DeleteGameRecord")
        .RequireAdminApiKey();
    }
}

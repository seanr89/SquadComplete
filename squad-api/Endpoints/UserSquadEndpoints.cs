using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using squad_api.DTOs;
using squad_api.Models;
using System;
using System.Linq;

namespace squad_api.Endpoints;

public static class UserSquadEndpoints
{
    public static void MapUserSquadEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/user-squads").WithTags(nameof(UserSquad));

        group.MapPost("/", async (CreateUserSquadDto dto, SquadContext db) =>
        {
            if (dto.Players == null || dto.Players.Count != 11)
            {
                return Results.BadRequest("A squad must contain exactly 11 players.");
            }

            // Find or create user
            var user = await db.Users.FirstOrDefaultAsync(u => u.BrowserIdentifierId == dto.BrowserIdentifierId);
            if (user == null)
            {
                user = new User
                {
                    BrowserIdentifierId = dto.BrowserIdentifierId,
                    Name = dto.UserName
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
            else if (!string.IsNullOrEmpty(dto.UserName) && user.Name != dto.UserName)
            {
                user.Name = dto.UserName;
                await db.SaveChangesAsync();
            }

            // Check if squad already submitted
            var existingSquad = await db.UserSquads
                .FirstOrDefaultAsync(us => us.UserId == user.Id && us.GameRecordId == dto.GameRecordId);

            if (existingSquad != null)
            {
                return Results.BadRequest("A squad has already been submitted for this game record.");
            }

            // Verify game record exists
            var gameRecordExists = await db.GameRecords.AnyAsync(gr => gr.Id == dto.GameRecordId);
            if (!gameRecordExists)
            {
                return Results.BadRequest("Invalid game record.");
            }

            // Verify formation exists
            var formationExists = await db.Formations.AnyAsync(f => f.Id == dto.FormationId);
            if (!formationExists)
            {
                return Results.BadRequest("Invalid formation.");
            }

            var userSquad = new UserSquad
            {
                UserId = user.Id,
                GameRecordId = dto.GameRecordId,
                FormationId = dto.FormationId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            db.UserSquads.Add(userSquad);
            await db.SaveChangesAsync(); // Save to get the Id

            foreach (var p in dto.Players)
            {
                var playerExists = await db.Players.AnyAsync(pl => pl.Id == p.PlayerId);
                if (!playerExists)
                {
                    // Option 1: Ignore invalid players.
                    // Option 2: Return bad request.
                    // Returning bad request is safer.
                    return Results.BadRequest($"Invalid player id {p.PlayerId}.");
                }

                db.UserSquadPlayers.Add(new UserSquadPlayer
                {
                    UserSquadId = userSquad.Id,
                    PlayerId = p.PlayerId,
                    Position = p.Position,
                    IsCaptain = p.IsCaptain,
                    IsViceCaptain = p.IsViceCaptain,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await db.SaveChangesAsync();

            return Results.Created($"/api/user-squads/{userSquad.Id}", new { userSquad.Id });
        })
        .WithName("CreateUserSquad");

        group.MapGet("/{gameRecordId:int}/leaderboard", async (int gameRecordId, SquadContext db) =>
        {
            var userSquads = await db.UserSquads
                .Include(us => us.User)
                .Include(us => us.UserSquadPlayers)
                    .ThenInclude(usp => usp.Player)
                .Where(us => us.GameRecordId == gameRecordId)
                .ToListAsync();

            // Needs to calculate avg rating based on player ratings from GameRecordPlayerStatisticDto or PlayerFixtureStatistic
            // For now, let's just return what we have mapped or calculate it manually if possible.
            // A more complex query might be needed to join with PlayerFixtureStatistic

             var fixtureIds = await db.GameRecordTags
                .Where(t => t.GameRecordId == gameRecordId)
                .Select(t => t.FixtureId)
                .Distinct()
                .ToListAsync();

            var teamIds = await db.GameRecordTags
                .Where(t => t.GameRecordId == gameRecordId)
                .Select(t => t.TeamId)
                .Distinct()
                .ToListAsync();

            var statistics = await db.PlayerFixtureStatistics
                .Where(s => fixtureIds.Contains(s.FixtureId) && s.TeamId != null && teamIds.Contains(s.TeamId.Value))
                .ToListAsync();


            var result = userSquads.Select(us => {
                var ratingSum = us.UserSquadPlayers.Sum(usp => {
                    var stat = statistics.FirstOrDefault(s => s.PlayerId == usp.PlayerId);
                    return stat?.Rating ?? 0m;
                });
                var avgRating = us.UserSquadPlayers.Count > 0 ? ratingSum / us.UserSquadPlayers.Count : 0m;

                return new {
                    id = us.Id.ToString(),
                    playerName = us.User?.Name ?? "Anonymous",
                    teamAverageRating = avgRating,
                    squad = us.UserSquadPlayers.Select(usp => new {
                        id = usp.Player?.Id.ToString(),
                        name = usp.Player?.Name,
                        position = usp.Position,
                        rating = statistics.FirstOrDefault(s => s.PlayerId == usp.PlayerId)?.Rating ?? 0m
                    })
                };
            }).OrderByDescending(r => r.teamAverageRating).ToList();


            return Results.Ok(result);
        })
        .WithName("GetLeaderboardForGameRecord");
    }
}

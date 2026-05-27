using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using squad_api.DTOs;
using squad_api.Models;
using System;
using System.Linq;

namespace squad_api.Endpoints;

public static class EventEndpoints
{
    public static void MapEventEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/events").WithTags("Events");

        // GET /api/events - Retrieve recent events (ordered by date descending)
        group.MapGet("/", async (SquadContext db, int limit = 50) =>
        {
            var events = await db.Events
                .OrderByDescending(e => e.CreatedAt)
                .Take(limit)
                .ToListAsync();
            return Results.Ok(events);
        });

        // POST /api/events - Log a new event
        group.MapPost("/", async (EventDto eventDto, SquadContext db) =>
        {
            if (string.IsNullOrWhiteSpace(eventDto.Title))
            {
                return Results.BadRequest("Title is required.");
            }
            if (string.IsNullOrWhiteSpace(eventDto.Message))
            {
                return Results.BadRequest("Message is required.");
            }
            if (string.IsNullOrWhiteSpace(eventDto.Level))
            {
                return Results.BadRequest("Level is required.");
            }

            var newEvent = new Event
            {
                Title = eventDto.Title,
                Message = eventDto.Message,
                Level = eventDto.Level,
                CreatedAt = DateTime.UtcNow
            };

            db.Events.Add(newEvent);
            await db.SaveChangesAsync();

            return Results.Created($"/api/events/{newEvent.Id}", newEvent);
        });
    }
}

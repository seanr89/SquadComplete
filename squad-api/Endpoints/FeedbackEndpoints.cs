using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using squad_api.DTOs;
using squad_api.Models;
using System.ComponentModel.DataAnnotations;

namespace squad_api.Endpoints;

public static class FeedbackEndpoints
{
    public static void MapFeedbackEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/feedback").WithTags("Feedback");

        group.MapPost("/", async (FeedbackDto feedbackDto, SquadContext db) =>
        {
            if (string.IsNullOrWhiteSpace(feedbackDto.Name))
            {
                return Results.BadRequest("Name is required.");
            }
            if (string.IsNullOrWhiteSpace(feedbackDto.Email))
            {
                return Results.BadRequest("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(feedbackDto.Message))
            {
                return Results.BadRequest("Message is required.");
            }

            var feedback = new Feedback
            {
                Name = feedbackDto.Name,
                Email = feedbackDto.Email,
                Message = feedbackDto.Message,
                CreatedAt = DateTime.UtcNow
            };

            db.Feedback.Add(feedback);
            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }
}

using Microsoft.EntityFrameworkCore;
using squad_api.Models;

namespace squad_api.Endpoints;

public static class FormationEndpoints
{
    /// <summary>
    /// Maps the formation management endpoints for the API.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapFormationEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/formations").WithTags(nameof(Formation));

        /// <summary>
        /// Retrieves all formations.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <returns>A list of all formations.</returns>
        group.MapGet("/", async (SquadContext db) =>
        {
            return await db.Formations.ToListAsync();
        })
        .WithName("GetAllFormations");

        /// <summary>
        /// Retrieves a specific formation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the formation.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The requested formation if found; otherwise, a 404 Not Found response.</returns>
        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await db.Formations.FindAsync(id)
                is Formation model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName("GetFormationById");

        /// <summary>
        /// Updates an existing formation's details.
        /// </summary>
        /// <param name="id">The unique identifier of the formation to update.</param>
        /// <param name="inputFormation">The updated formation data.</param>
        /// <param name="db">The database context.</param>
        /// <returns>A 204 No Content response if successful; otherwise, a 404 Not Found response.</returns>
        group.MapPut("/{id}", async (int id, Formation inputFormation, SquadContext db) =>
        {
            var foundModel = await db.Formations.FindAsync(id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            // Update properties
            foundModel.Name = inputFormation.Name;
            foundModel.Defence = inputFormation.Defence;
            foundModel.Midfield = inputFormation.Midfield;
            foundModel.Attack = inputFormation.Attack;

            await db.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("UpdateFormation");

        /// <summary>
        /// Creates a new formation.
        /// </summary>
        /// <param name="formation">The formation data to create.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The newly created formation with a 201 Created response.</returns>
        group.MapPost("/", async (Formation formation, SquadContext db) =>
        {
            db.Formations.Add(formation);
            await db.SaveChangesAsync();
            return Results.Created($"/api/formations/{formation.Id}", formation);
        })
        .WithName("CreateFormation");

        /// <summary>
        /// Deletes a specific formation by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the formation to delete.</param>
        /// <param name="db">The database context.</param>
        /// <returns>The deleted formation if successful; otherwise, a 404 Not Found response.</returns>
        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.Formations.FindAsync(id) is Formation formation)
            {
                db.Formations.Remove(formation);
                await db.SaveChangesAsync();
                return Results.Ok(formation);
            }

            return Results.NotFound();
        })
        .WithName("DeleteFormation");
    }
}

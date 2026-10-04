using Microsoft.EntityFrameworkCore;
using squad_api.Auth;
using squad_api.Models;

namespace squad_api.Endpoints;

/// <summary>
/// Describes the entity-specific parts of a standard single-<c>int</c>-key CRUD resource.
/// Everything else (routing, 404/201/204 handling, admin API key on writes) lives in
/// <see cref="CrudEndpointExtensions.MapCrud{TEntity, TDto}"/>.
/// </summary>
/// <typeparam name="TEntity">The EF entity. Must expose an <c>int Id</c> primary key.</typeparam>
/// <typeparam name="TDto">The request contract bound for POST/PUT.</typeparam>
public sealed class CrudEndpointConfig<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    /// <summary>Singular name used to build operation names, e.g. "League" → <c>GetLeagueById</c>.</summary>
    public required string Singular { get; init; }

    /// <summary>Plural name used to build the list operation name, e.g. "Leagues" → <c>GetAllLeagues</c>.</summary>
    public required string Plural { get; init; }

    /// <summary>Maps a create request to a new entity.</summary>
    public required Func<TDto, TEntity> Create { get; init; }

    /// <summary>Copies updatable fields from a PUT request onto the tracked entity.</summary>
    public required Action<TEntity, TDto> ApplyUpdate { get; init; }

    /// <summary>Optionally adds <c>.Include()</c>s to the read (GET) queries.</summary>
    public Func<IQueryable<TEntity>, IQueryable<TEntity>>? Includes { get; init; }
}

public static class CrudEndpointExtensions
{
    /// <summary>
    /// Maps GET (all / by id), PUT, POST and DELETE for an entity keyed by a single <c>int Id</c>.
    /// Writes require the admin API key.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="basePath">The resource route, e.g. <c>/api/leagues</c>.</param>
    /// <param name="config">The entity-specific mapping.</param>
    public static void MapCrud<TEntity, TDto>(
        this IEndpointRouteBuilder routes,
        string basePath,
        CrudEndpointConfig<TEntity, TDto> config)
        where TEntity : class
        where TDto : class
    {
        var group = routes.MapGroup(basePath).WithTags(typeof(TEntity).Name);

        IQueryable<TEntity> ReadQuery(SquadContext db)
        {
            var query = db.Set<TEntity>().AsNoTracking();
            return config.Includes is null ? query : config.Includes(query);
        }

        group.MapGet("/", async (SquadContext db) =>
        {
            return await ReadQuery(db).ToListAsync();
        })
        .WithName($"GetAll{config.Plural}");

        group.MapGet("/{id}", async (int id, SquadContext db) =>
        {
            return await ReadQuery(db).FirstOrDefaultAsync(x => EF.Property<int>(x, "Id") == id)
                is TEntity model
                    ? Results.Ok(model)
                    : Results.NotFound();
        })
        .WithName($"Get{config.Singular}ById");

        // The PUT/POST handlers are assigned to locals rather than passed inline because the
        // RouteHandlerAnalyzer throws (AD0001) on lambdas with a type-parameter-typed parameter.
        // Routing and OpenAPI metadata are unaffected.
        var update = async (int id, TDto input, SquadContext db) =>
        {
            var foundModel = await db.Set<TEntity>().FindAsync(id);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            config.ApplyUpdate(foundModel, input);

            await db.SaveChangesAsync();

            return Results.NoContent();
        };

        group.MapPut("/{id}", update)
        .WithName($"Update{config.Singular}")
        .RequireAdminApiKey();

        var create = async (TDto input, SquadContext db) =>
        {
            var entity = config.Create(input);

            db.Set<TEntity>().Add(entity);
            await db.SaveChangesAsync();

            var id = db.Entry(entity).Property<int>("Id").CurrentValue;
            return Results.Created($"{basePath}/{id}", entity);
        };

        group.MapPost("/", create)
        .WithName($"Create{config.Singular}")
        .RequireAdminApiKey();

        group.MapDelete("/{id}", async (int id, SquadContext db) =>
        {
            if (await db.Set<TEntity>().FindAsync(id) is TEntity entity)
            {
                db.Set<TEntity>().Remove(entity);
                await db.SaveChangesAsync();
                return Results.Ok(entity);
            }

            return Results.NotFound();
        })
        .WithName($"Delete{config.Singular}")
        .RequireAdminApiKey();
    }
}

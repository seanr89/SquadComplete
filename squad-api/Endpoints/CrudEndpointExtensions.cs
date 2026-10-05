using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using squad_api.Auth;
using squad_api.Models;

namespace squad_api.Endpoints;

/// <summary>
/// Route-bound primary key of a CRUD resource. Implemented by a record whose properties match the
/// route parameters of the key template (e.g. <c>/{fixtureId}/{playerId}</c>) and which is bound with
/// <c>[AsParameters]</c>, so the generated OpenAPI path parameters stay typed and named.
/// </summary>
/// <typeparam name="TEntity">The entity the key identifies.</typeparam>
public interface ICrudKey<TEntity> where TEntity : class
{
    /// <summary>Key values in primary-key order, as accepted by <c>DbSet.FindAsync</c>.</summary>
    object[] Values { get; }

    /// <summary>Predicate matching the entity with this key, for queries that need <c>.Include()</c>s.</summary>
    Expression<Func<TEntity, bool>> Predicate { get; }
}

/// <summary>The default single-<c>int</c> key, bound from <c>/{id}</c>.</summary>
public sealed record IdKey<TEntity>(int Id) : ICrudKey<TEntity> where TEntity : class
{
    public object[] Values => [Id];

    public Expression<Func<TEntity, bool>> Predicate => x => EF.Property<int>(x, "Id") == Id;
}

/// <summary>
/// Describes the entity-specific parts of a CRUD resource.
/// Everything else (routing, 404/201/204 handling, admin API key on writes) lives in
/// <see cref="CrudEndpointExtensions.MapCrud{TEntity, TDto, TResponse, TKey}"/>.
/// </summary>
/// <typeparam name="TEntity">The EF entity.</typeparam>
/// <typeparam name="TDto">The request contract bound for POST/PUT.</typeparam>
/// <typeparam name="TResponse">The response contract returned instead of the EF entity.</typeparam>
public sealed class CrudEndpointConfig<TEntity, TDto, TResponse>
    where TEntity : class
    where TDto : class
    where TResponse : class
{
    /// <summary>Singular name used to build operation names, e.g. "League" → <c>GetLeagueById</c>.</summary>
    public required string Singular { get; init; }

    /// <summary>Plural name used to build the list operation name, e.g. "Leagues" → <c>GetAllLeagues</c>.</summary>
    public required string Plural { get; init; }

    /// <summary>Maps a create request to a new entity.</summary>
    public required Func<TDto, TEntity> Create { get; init; }

    /// <summary>Copies updatable fields from a PUT request onto the tracked entity.</summary>
    public required Action<TEntity, TDto> ApplyUpdate { get; init; }

    /// <summary>Maps an entity to the response contract. Endpoints never serialize EF entities directly.</summary>
    public required Func<TEntity, TResponse> ToResponse { get; init; }

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
    public static void MapCrud<TEntity, TDto, TResponse>(
        this IEndpointRouteBuilder routes,
        string basePath,
        CrudEndpointConfig<TEntity, TDto, TResponse> config)
        where TEntity : class
        where TDto : class
        where TResponse : class
        => routes.MapCrud<TEntity, TDto, TResponse, IdKey<TEntity>>(basePath, "/{id}", config);

    /// <summary>
    /// Maps GET (all / by key), PUT, POST and DELETE for an entity identified by <typeparamref name="TKey"/>
    /// (single or composite). Writes require the admin API key.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    /// <param name="basePath">The resource route, e.g. <c>/api/player-fixture-statistics</c>.</param>
    /// <param name="keyRoute">The key route template appended to <paramref name="basePath"/>, e.g. <c>/{fixtureId}/{playerId}</c>.
    /// Its parameters must match the properties of <typeparamref name="TKey"/>, in primary-key order.</param>
    /// <param name="config">The entity-specific mapping.</param>
    public static void MapCrud<TEntity, TDto, TResponse, TKey>(
        this IEndpointRouteBuilder routes,
        string basePath,
        string keyRoute,
        CrudEndpointConfig<TEntity, TDto, TResponse> config)
        where TEntity : class
        where TDto : class
        where TResponse : class
        where TKey : ICrudKey<TEntity>
    {
        var group = routes.MapGroup(basePath).WithTags(typeof(TEntity).Name);

        IQueryable<TEntity> ReadQuery(SquadContext db)
        {
            var query = db.Set<TEntity>().AsNoTracking();
            return config.Includes is null ? query : config.Includes(query);
        }

        group.MapGet("/", async (SquadContext db) =>
        {
            var entities = await ReadQuery(db).ToListAsync();
            return entities.Select(config.ToResponse).ToList();
        })
        .WithName($"GetAll{config.Plural}");

        // The handlers taking TKey/TDto are assigned to locals rather than passed inline because the
        // RouteHandlerAnalyzer throws (AD0001) on lambdas with a type-parameter-typed parameter.
        // Routing and OpenAPI metadata are unaffected.
        var getByKey = async ([AsParameters] TKey key, SquadContext db) =>
        {
            return await ReadQuery(db).FirstOrDefaultAsync(key.Predicate)
                is TEntity model
                    ? Results.Ok(config.ToResponse(model))
                    : Results.NotFound();
        };

        group.MapGet(keyRoute, getByKey)
        .WithName($"Get{config.Singular}ById");

        var update = async ([AsParameters] TKey key, TDto input, SquadContext db) =>
        {
            var foundModel = await db.Set<TEntity>().FindAsync(key.Values);

            if (foundModel is null)
            {
                return Results.NotFound();
            }

            config.ApplyUpdate(foundModel, input);

            await db.SaveChangesAsync();

            return Results.NoContent();
        };

        group.MapPut(keyRoute, update)
        .WithName($"Update{config.Singular}")
        .RequireAdminApiKey();

        var create = async (TDto input, SquadContext db) =>
        {
            var entity = config.Create(input);

            db.Set<TEntity>().Add(entity);
            await db.SaveChangesAsync();

            var entry = db.Entry(entity);
            var key = entry.Metadata.FindPrimaryKey()!.Properties.Select(p => entry.Property(p.Name).CurrentValue);
            return Results.Created($"{basePath}/{string.Join('/', key)}", config.ToResponse(entity));
        };

        group.MapPost("/", create)
        .WithName($"Create{config.Singular}")
        .RequireAdminApiKey();

        var delete = async ([AsParameters] TKey key, SquadContext db) =>
        {
            if (await db.Set<TEntity>().FindAsync(key.Values) is TEntity entity)
            {
                db.Set<TEntity>().Remove(entity);
                await db.SaveChangesAsync();
                return Results.Ok(config.ToResponse(entity));
            }

            return Results.NotFound();
        };

        group.MapDelete(keyRoute, delete)
        .WithName($"Delete{config.Singular}")
        .RequireAdminApiKey();
    }
}

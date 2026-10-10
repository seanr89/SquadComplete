using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using squad_api.DTOs;

namespace squad_api.Endpoints;

/// <summary>Composite primary key of a <see cref="PlayerFixtureStatistic"/>, bound from <c>/{fixtureId}/{playerId}</c>.</summary>
public sealed record PlayerFixtureStatisticKey(int FixtureId, int PlayerId) : ICrudKey<PlayerFixtureStatistic>
{
    public object[] Values => [FixtureId, PlayerId];

    public Expression<Func<PlayerFixtureStatistic, bool>> Predicate =>
        pfs => pfs.FixtureId == FixtureId && pfs.PlayerId == PlayerId;
}

public static class PlayerFixtureStatisticEndpoints
{
    /// <summary>
    /// Maps the player fixture statistic endpoints for the API, keyed by fixture ID and player ID.
    /// Reads include the related Fixture, Player and Team.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapPlayerFixtureStatisticEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapCrud<PlayerFixtureStatistic, PlayerFixtureStatisticDto, PlayerFixtureStatisticResponse, PlayerFixtureStatisticKey>(
            "/api/player-fixture-statistics",
            "/{fixtureId}/{playerId}",
            new CrudEndpointConfig<PlayerFixtureStatistic, PlayerFixtureStatisticDto, PlayerFixtureStatisticResponse>
            {
                Singular = "PlayerFixtureStatistic",
                ToResponse = PlayerFixtureStatisticResponse.From,
                Plural = "PlayerFixtureStatistics",
                Includes = query => query
                    .Include(pfs => pfs.Fixture)
                    .Include(pfs => pfs.Player)
                    .Include(pfs => pfs.Team),
                Create = dto => new PlayerFixtureStatistic
                {
                    FixtureId = dto.FixtureId,
                    PlayerId = dto.PlayerId,
                    TeamId = dto.TeamId,
                    Minutes = dto.Minutes,
                    Number = dto.Number,
                    Position = dto.Position,
                    Rating = dto.Rating,
                    IsCaptain = dto.IsCaptain,
                    IsSubstitute = dto.IsSubstitute
                },
                ApplyUpdate = (pfs, dto) =>
                {
                    pfs.TeamId = dto.TeamId;
                    pfs.Minutes = dto.Minutes;
                    pfs.Number = dto.Number;
                    pfs.Position = dto.Position;
                    pfs.Rating = dto.Rating;
                    pfs.IsCaptain = dto.IsCaptain;
                    pfs.IsSubstitute = dto.IsSubstitute;
                    pfs.UpdatedAt = DateTime.UtcNow;
                }
            });
    }
}

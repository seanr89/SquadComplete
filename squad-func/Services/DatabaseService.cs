using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace squad_func.Services;

public interface IDatabaseService
{
    /// <summary>
    /// Inserts or updates a team record.
    /// </summary>
    /// <param name="id">The team's unique ID.</param>
    /// <param name="name">The name of the team.</param>
    /// <param name="logo">The URL to the team's logo.</param>
    /// <param name="lastUpdate">The last time the team data was updated.</param>
    Task UpsertTeamAsync(int id, string name, string? logo, DateTime? lastUpdate);

    /// <summary>
    /// Inserts or updates a player record.
    /// </summary>
    /// <param name="id">The player's unique ID.</param>
    /// <param name="name">The name of the player.</param>
    /// <param name="photo">The URL to the player's photo.</param>
    Task UpsertPlayerAsync(int id, string name, string? photo);

    /// <summary>
    /// Inserts or updates statistics for a player in a specific fixture.
    /// </summary>
    /// <param name="fixtureId">The ID of the fixture.</param>
    /// <param name="teamId">The ID of the team.</param>
    /// <param name="playerId">The ID of the player.</param>
    /// <param name="minutes">The number of minutes played.</param>
    /// <param name="number">The player's jersey number.</param>
    /// <param name="position">The player's position.</param>
    /// <param name="rating">The player's match rating.</param>
    /// <param name="isCaptain">Whether the player was the captain.</param>
    /// <param name="isSubstitute">Whether the player was a substitute.</param>
    Task UpsertPlayerStatsAsync(int fixtureId, int teamId, int playerId, int? minutes, int? number, string? position, decimal? rating, bool isCaptain, bool isSubstitute);
}

public class DatabaseService : IDatabaseService
{
    private readonly SquadContext _context;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(SquadContext context, ILoggerFactory loggerFactory)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = loggerFactory.CreateLogger<DatabaseService>();
    }

    /// <inheritdoc />
    public async Task UpsertTeamAsync(int id, string name, string? logo, DateTime? lastUpdate)
    {
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO teams (id, name, logo, last_update)
                VALUES ({id}, {name}, {logo}, {lastUpdate})
                ON CONFLICT (id) DO UPDATE SET
                    name = EXCLUDED.name,
                    logo = EXCLUDED.logo,
                    last_update = EXCLUDED.last_update,
                    updated_at = CURRENT_TIMESTAMP;");
            
            _logger.LogInformation("Successfully upserted team {TeamId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting team {TeamId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpsertPlayerAsync(int id, string name, string? photo)
    {
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO players (id, name, photo)
                VALUES ({id}, {name}, {photo})
                ON CONFLICT (id) DO UPDATE SET
                    name = EXCLUDED.name,
                    photo = EXCLUDED.photo,
                    updated_at = CURRENT_TIMESTAMP;");
            
            _logger.LogInformation("Successfully upserted player {PlayerId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting player {PlayerId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpsertPlayerStatsAsync(int fixtureId, int teamId, int playerId, int? minutes, int? number, string? position, decimal? rating, bool isCaptain, bool isSubstitute)
    {
        try
        {
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                INSERT INTO player_fixture_statistics 
                (fixture_id, team_id, player_id, minutes, number, position, rating, is_captain, is_substitute)
                VALUES ({fixtureId}, {teamId}, {playerId}, {minutes}, {number}, {position}, {rating}, {isCaptain}, {isSubstitute})
                ON CONFLICT (fixture_id, player_id) DO UPDATE SET
                    team_id = EXCLUDED.team_id,
                    minutes = EXCLUDED.minutes,
                    number = EXCLUDED.number,
                    position = EXCLUDED.position,
                    rating = EXCLUDED.rating,
                    is_captain = EXCLUDED.is_captain,
                    is_substitute = EXCLUDED.is_substitute,
                    updated_at = CURRENT_TIMESTAMP;");
            
            _logger.LogInformation("Successfully upserted stats for player {PlayerId} in fixture {FixtureId}", playerId, fixtureId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting stats for player {PlayerId} in fixture {FixtureId}", playerId, fixtureId);
            throw;
        }
    }
}

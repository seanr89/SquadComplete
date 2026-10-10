using squad_api.DTOs;

namespace squad_api.Endpoints;

public static class PlayerEndpoints
{
    /// <summary>
    /// Maps the player management endpoints for the API.
    /// </summary>
    /// <param name="routes">The endpoint route builder.</param>
    public static void MapPlayerEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapCrud("/api/players", new CrudEndpointConfig<Player, PlayerDto, PlayerResponse>
        {
            Singular = "Player",
            ToResponse = PlayerResponse.From,
            Plural = "Players",
            Create = dto => new Player
            {
                Name = dto.Name,
                Photo = dto.Photo,
                ApiId = dto.ApiId
            },
            ApplyUpdate = (player, dto) =>
            {
                player.Name = dto.Name;
                player.Photo = dto.Photo;
                player.ApiId = dto.ApiId;
                player.UpdatedAt = DateTime.UtcNow;
            }
        });
    }
}

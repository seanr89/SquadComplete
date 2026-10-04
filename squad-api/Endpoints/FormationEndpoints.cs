using squad_api.DTOs;
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
        routes.MapCrud("/api/formations", new CrudEndpointConfig<Formation, FormationDto>
        {
            Singular = "Formation",
            Plural = "Formations",
            Create = dto => new Formation
            {
                Name = dto.Name,
                Defence = dto.Defence,
                Midfield = dto.Midfield,
                Attack = dto.Attack
            },
            ApplyUpdate = (formation, dto) =>
            {
                formation.Name = dto.Name;
                formation.Defence = dto.Defence;
                formation.Midfield = dto.Midfield;
                formation.Attack = dto.Attack;
            }
        });
    }
}

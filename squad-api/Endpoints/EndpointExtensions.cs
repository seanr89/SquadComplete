using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace squad_api.Endpoints;

public static class EndpointExtensions
{
    public static void MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapLeagueEndpoints();
        app.MapFixtureEndpoints();
        app.MapPlayerEndpoints();
        app.MapPlayerFixtureStatisticEndpoints();
        app.MapFormationEndpoints();
        app.MapGameRecordEndpoints();
        app.MapFeedbackEndpoints();
        app.MapEventEndpoints();
        app.MapUserSquadEndpoints();
        app.MapStatisticsEndpoints();
    }
}

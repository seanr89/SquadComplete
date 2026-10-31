using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace squad_api.Endpoints;

public static class EndpointExtensions
{
    /// <summary>
    /// Maps all API endpoints.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    public static void MapAllEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapFeedbackEndpoints();
        app.MapUserSquadEndpoints();
    }
}

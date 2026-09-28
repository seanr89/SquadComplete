using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using squad_func.Models;

namespace Squad.Function;

public class GetFixture(ILogger<GetFixture> logger, SquadContext context)
{
    private readonly ILogger<GetFixture> _logger = logger;
    private readonly SquadContext _context = context;

    // Mirrors squad-api's GET /api/fixtures/{id} (fixture + league) so the client can swap base URLs.
    [Function("GetFixture")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fixtures/{id:int}")] HttpRequest req,
        int id)
    {
        try
        {
            var fixture = await _context.Fixtures
                .AsNoTracking()
                .Include(f => f.League)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (fixture is null)
            {
                return new NotFoundResult();
            }

            // Historic fixtures don't change, so let the browser/CDN reuse the response.
            req.HttpContext.Response.Headers.CacheControl = "public, max-age=3600";

            return new OkObjectResult(fixture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load fixture {FixtureId}.", id);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}

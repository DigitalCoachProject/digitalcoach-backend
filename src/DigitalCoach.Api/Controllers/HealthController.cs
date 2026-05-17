using DigitalCoach.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Route("health")]
[Route("api/health")]
public sealed class HealthController(DigitalCoachDbContext dbContext, IWebHostEnvironment environment) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        return await BuildHealthResponseAsync(cancellationToken);
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        return await BuildHealthResponseAsync(cancellationToken);
    }

    private async Task<IActionResult> BuildHealthResponseAsync(CancellationToken cancellationToken)
    {
        var databaseHealthy = await CanConnectToDatabaseAsync(cancellationToken);
        var status = databaseHealthy ? "healthy" : "unhealthy";
        var response = new
        {
            status,
            service = "DigitalCoach.Api",
            environment = environment.EnvironmentName,
            timestampUtc = DateTime.UtcNow,
            checks = new
            {
                api = "healthy",
                sqlServer = databaseHealthy ? "healthy" : "unhealthy"
            }
        };

        return databaseHealthy
            ? Ok(response)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, response);
    }

    private async Task<bool> CanConnectToDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}

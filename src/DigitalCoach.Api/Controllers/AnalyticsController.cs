using DigitalCoach.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/analytics")]
public sealed class AnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("productivity")]
    public async Task<IActionResult> GetProductivityOverview([FromQuery] int userId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var overview = await analyticsService.GetProductivityOverviewAsync(userId, from, to, cancellationToken);
        return Ok(overview);
    }
}

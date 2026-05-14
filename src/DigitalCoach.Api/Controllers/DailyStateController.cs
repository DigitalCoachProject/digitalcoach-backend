using DigitalCoach.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/daily-state")]
public sealed class DailyStateController(IDailyStateService dailyStateService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        _ = dailyStateService;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

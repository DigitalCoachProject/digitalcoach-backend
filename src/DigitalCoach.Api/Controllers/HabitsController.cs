using DigitalCoach.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/habits")]
public sealed class HabitsController(IHabitService habitService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        _ = habitService;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

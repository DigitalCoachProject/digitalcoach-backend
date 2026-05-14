using DigitalCoach.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        _ = taskService;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

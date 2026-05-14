using DigitalCoach.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalCoach.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recommendations")]
public sealed class RecommendationsController(IRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        _ = recommendationService;
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}

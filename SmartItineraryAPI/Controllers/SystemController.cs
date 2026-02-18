using Microsoft.AspNetCore.Mvc;
using SmartItineraryAPI.Models.Responses;

namespace SmartItineraryAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new HealthResponse
        {
            Environment = "Development",
            Service = "Smart Itinerary API",
            Status = "Healthy",
            Timestamp = DateTime.UtcNow
        });
    }
}

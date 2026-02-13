using Microsoft.AspNetCore.Mvc;
using SmartItineraryAPI.Models.Responses;

namespace SmartItineraryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
}

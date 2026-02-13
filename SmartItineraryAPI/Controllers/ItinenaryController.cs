using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SmartItineraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItinenaryController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult GetItinerary()
        {
            return Ok();
        }
    }
}

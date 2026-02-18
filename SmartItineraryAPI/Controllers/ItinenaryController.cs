using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Models.Requests;
using SmartItineraryAPI.Models.Responses;

namespace SmartItineraryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItineraryController(IItineraryGenerator generator) : ControllerBase
{
    private readonly IItineraryGenerator _generator = generator;

    [EnableRateLimiting("itinerary-policy")]
    [HttpPost]
    public async Task<ActionResult<ItineraryResponse>> Generate(
        [FromBody] ItineraryRequest request,
        CancellationToken cancellationToken)
    {
        ItineraryResponse result =
            await _generator.GenerateAsync(request, cancellationToken);

        return Ok(result);
    }
}

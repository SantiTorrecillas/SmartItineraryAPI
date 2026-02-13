namespace SmartItineraryAPI.Models.Responses;

public class HealthResponse
{
    public string Status { get; init; } = default!;
    public string Service { get; init; } = default!;
    public string? Environment { get; init; }
    public DateTime Timestamp { get; init; }
}

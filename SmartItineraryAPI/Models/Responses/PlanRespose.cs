namespace SmartItineraryAPI.Models.Responses;

public class PlanResponse
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public double DurationHours { get; init; }
    public decimal EstimatedPrice { get; init; }
}

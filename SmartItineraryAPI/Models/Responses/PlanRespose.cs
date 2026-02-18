namespace SmartItineraryAPI.Models.Responses;

public class PlanResponse
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public decimal EstimatedPrice { get; init; }
}

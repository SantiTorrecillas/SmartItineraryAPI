namespace SmartItineraryAPI.Application.Results;

public class PlanItem
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;

    public TimeSpan Duration { get; init; }

    public decimal EstimatedPrice { get; init; }
}

namespace SmartItineraryAPI.Application.Results;

public class ItineraryDay
{
    public int DayNumber { get; init; }

    public IReadOnlyList<PlanItem> Plans { get; init; } = new List<PlanItem>();
}

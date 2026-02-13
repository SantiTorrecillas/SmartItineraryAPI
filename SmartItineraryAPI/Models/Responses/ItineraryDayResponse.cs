namespace SmartItineraryAPI.Models.Responses;

public class ItineraryDayResponse
{
    public int DayNumber { get; init; }
    public List<PlanResponse> Plans { get; init; } = new();
}

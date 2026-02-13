namespace SmartItineraryAPI.Models.Responses;

public class ItineraryResponse
{
    public string City { get; init; } = default!;
    public int Days { get; init; }

    public List<ItineraryDayResponse> DaysPlan { get; init; } = new();
}

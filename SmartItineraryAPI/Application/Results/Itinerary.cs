namespace SmartItineraryAPI.Application.Results;

public class Itinerary
{
    public IReadOnlyList<ItineraryDay> Days { get; init; } = [];
}

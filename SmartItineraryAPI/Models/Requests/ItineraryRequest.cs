namespace SmartItineraryAPI.Models.Requests;

public class ItineraryRequest
{
    public string City { get; init; } = default!;
    public int Days { get; init; }
    public decimal Budget { get; init; }
}

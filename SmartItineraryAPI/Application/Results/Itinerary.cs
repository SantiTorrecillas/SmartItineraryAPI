namespace SmartItineraryAPI.Application.Results;

public class Itinerary
{
    public IReadOnlyList<ItineraryDay> Days { get; init; } = [];

    // Metadata LLM
    public string? Model { get; init; }
    public int? TokensUsed { get; init; }
    public string? RawOutput { get; init; }
}

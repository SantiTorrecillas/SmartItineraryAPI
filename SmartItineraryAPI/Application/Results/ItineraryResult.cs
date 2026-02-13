namespace SmartItineraryAPI.Application.Results;

public class ItineraryResult
{
    public string City { get; init; } = default!;
    public int Days { get; init; }

    public IReadOnlyList<ItineraryDay> DaysPlan { get; init; } = new List<ItineraryDay>();

    // Metadata LLM
    public string? Model { get; init; }
    public int? TokensUsed { get; init; }
    public string? RawOutput { get; init; }
}

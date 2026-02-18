namespace SmartItineraryAPI.Infrastructure.AI;

public class OpenAiOptions
{
    public string ApiKey { get; set; } = default!;
    public string Model { get; set; } = "gpt-mini";
}

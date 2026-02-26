using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Application.Results;
using SmartItineraryAPI.Models.Requests;
using SmartItineraryAPI.Models.Responses;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SmartItineraryAPI.Infrastructure.AI;

public class OpenAiItineraryGenerator(
    OpenAIClient client,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiItineraryGenerator> logger,
    IMemoryCache cache) : IItineraryGenerator
{
    private readonly OpenAIClient _client = client;
    private readonly OpenAiOptions _options = options.Value;
    private readonly ILogger<OpenAiItineraryGenerator> _logger = logger;
    private readonly IMemoryCache _cache = cache;

    private static readonly string[] jsonSerializable = ["days"];
    private static readonly string[] jsonSerializableDaysArray = ["dayNumber", "plans"];
    private static readonly string[] jsonSerializablePlansArray = ["time", "activity", "price"];

    public async Task<ItineraryResponse> GenerateAsync(
    ItineraryRequest request,
    CancellationToken cancellationToken)
    {
        string key = GenerateKey(request);

        var result = await _cache.GetOrCreateAsync(key, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
            return await GenerateFromOpenAiAsync(request, cancellationToken);
        });

        return result ?? throw new InvalidOperationException("Itinerary generator produced no result.");
    }

    public async Task<ItineraryResponse> GenerateFromOpenAiAsync(
        ItineraryRequest request,
        CancellationToken cancellationToken)
    {
        BinaryData schema = GetSchema(request);

        ChatClient chatClient = _client.GetChatClient(_options.Model);

        List<ChatMessage> messages =
        [
            ChatMessage.CreateSystemMessage(
                "You are a professional travel planner. Be concise and realistic."
            ),
           ChatMessage.CreateUserMessage(
               $"""
                Create an itinerary in {request.City}.
                The itinerary MUST contain EXACTLY {request.Days} days.
                
                The total estimated price of all activities should aim to be close
                to the total budget of {request.Budget}, spending at least 90%
                ONLY IF it is realistic.
                
                IMPORTANT:
                - Realism ALWAYS has priority over matching the budget.
                - Do NOT invent or inflate prices.
                - If the budget cannot be fully spent realistically, spend as much
                  as possible on valid premium activities and stop.
                
                Price constraints:
                - Breakfast / coffee / snacks: max 30 USD
                - Lunch: max 60 USD
                - Dinner: max 100 USD
                - Premium dinner (Michelin / tasting menu): max 300 USD
                - Museums / attractions: max 80 USD
                - Half-day guided tours: max 150 USD
                - Full-day guided tours: max 300 USD
                - Local transport per day: max 50 USD
                
                If extra budget remains, prioritize:
                - Fine dining
                - Private tours
                - Day trips
                - Luxury experiences
                
                Do NOT increase prices of basic activities to absorb budget.
                
                The "days" array MUST contain exactly {request.Days} objects.
                Each object must have:
                - dayNumber (starting at 1)
                - plans (at least 3 activities)
                
                Return only valid JSON.
                """
            )
        ];


        ChatCompletionOptions options = new()
        {
            MaxOutputTokenCount = 1000,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "Itinerary",
                schema,
                jsonSchemaIsStrict: true)
        };


        System.ClientModel.ClientResult<ChatCompletion> response =
            await chatClient.CompleteChatAsync(
                messages,
                options,
                cancellationToken);

        _logger.LogInformation(
            "OpenAI request completed. Model: {Model} | Tokens used: {Tokens} | City: {City} | Days: {Days} | Budget: {Budget}",
            _options.Model,
            response.Value.Usage.TotalTokenCount,
            request.City,
            request.Days,
            request.Budget);

        string content = response.Value.Content[0].Text;

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        JsonSerializerOptions jsonOptions = jsonSerializerOptions;

        Itinerary? aiResponse =
            JsonSerializer.Deserialize<Itinerary>(content, jsonOptions) ?? throw new Exception("Failed to deserialize itinerary JSON.");

        ItineraryResponse result = new()
        {
            City = request.City,
            Days = request.Days,
            DaysPlan = [.. aiResponse.Days
                .Select(day => new ItineraryDayResponse
                {
                    DayNumber = day.DayNumber,
                    Plans = [.. day.Plans
                        .Select(plan => new PlanResponse
                        {
                            Title = plan.Activity,
                            Description = $"Scheduled at {plan.Time}",
                            EstimatedPrice = plan.Price
                        })]
                })]
        };

        return result;
    }

    private static BinaryData GetSchema(ItineraryRequest request)
    {
        return BinaryData.FromObjectAsJson(
        new
        {
            type = "object",
            properties = new
            {
                days = new
                {
                    type = "array",
                    minItems = request.Days,
                    maxItems = request.Days,
                    items = new
                    {
                        type = "object",
                        properties = new
                        {
                            dayNumber = new { type = "integer" },
                            plans = new
                            {
                                type = "array",
                                items = new
                                {
                                    type = "object",
                                    properties = new
                                    {
                                        time = new { type = "string" },
                                        activity = new { type = "string" },
                                        price = new { type = "number" }
                                    },
                                    required = jsonSerializablePlansArray,
                                    additionalProperties = false
                                }
                            }
                        },
                        required = jsonSerializableDaysArray,
                        additionalProperties = false
                    }
                }
            },
            required = jsonSerializable,
            additionalProperties = false
        });
    }

    private static string GenerateKey(ItineraryRequest request)
    {
        string json = JsonSerializer.Serialize(request);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return $"itinerary:{Convert.ToHexString(hash)}";
    }
}

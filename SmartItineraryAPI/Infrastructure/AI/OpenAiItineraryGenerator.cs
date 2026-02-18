using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Application.Results;
using SmartItineraryAPI.Models.Requests;
using SmartItineraryAPI.Models.Responses;
using System.Text.Json;

namespace SmartItineraryAPI.Infrastructure.AI
{
    public class OpenAiItineraryGenerator(
        OpenAIClient client,
        IOptions<OpenAiOptions> options) : IItineraryGenerator
    {
        private readonly OpenAIClient _client = client;
        private readonly OpenAiOptions _options = options.Value;
        private static readonly string[] jsonSerializable = ["days"];
        private static readonly string[] jsonSerializableDaysArray = ["dayNumber", "plans"];
        private static readonly string[] jsonSerializablePlansArray = ["time", "activity", "price"];

        public async Task<ItineraryResponse> GenerateAsync(
            ItineraryRequest request,
            CancellationToken cancellationToken)
        {
            BinaryData schema = BinaryData.FromObjectAsJson(
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

                    The total estimated price of all activities across all days
                    MUST be as close as possible to the total budget of {request.Budget}.
                    You should spend at least 90% of the total budget.
                    Do not significantly under-spend the budget.

                    The "days" array MUST contain exactly {request.Days} objects.
                    Each object must have:
                    - dayNumber (starting at 1 and sequential)
                    - plans (array with at least 3 activities)

                    Prices must be realistic for the city.
                    Distribute the budget logically across all days.

                    Do not return fewer days.
                    Do not return more days.
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
    }
}

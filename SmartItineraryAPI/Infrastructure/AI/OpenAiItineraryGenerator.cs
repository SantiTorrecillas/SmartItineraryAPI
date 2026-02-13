using SmartItineraryAPI.Application.Interfaces;
using SmartItineraryAPI.Application.Results;
using SmartItineraryAPI.Models.Requests;

namespace SmartItineraryAPI.Infrastructure.AI
{
    public class OpenAiItineraryGenerator : IItineraryGenerator
    {
        // implementación real contra OpenAI
        public Task<ItineraryResult> GenerateAsync(ItineraryRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
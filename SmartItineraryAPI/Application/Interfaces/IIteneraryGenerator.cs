using SmartItineraryAPI.Application.Results;
using SmartItineraryAPI.Models.Requests;

namespace SmartItineraryAPI.Application.Interfaces
{
    public interface IItineraryGenerator
    {
        Task<ItineraryResult> GenerateAsync(ItineraryRequest request);
    }
}
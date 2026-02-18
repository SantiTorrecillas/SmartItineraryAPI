using SmartItineraryAPI.Models.Requests;
using SmartItineraryAPI.Models.Responses;

namespace SmartItineraryAPI.Application.Interfaces;

public interface IItineraryGenerator
{
    Task<ItineraryResponse> GenerateAsync(ItineraryRequest request, CancellationToken cancellationToken);
}
using FluentValidation;
using SmartItineraryAPI.Models.Requests;

namespace SmartItineraryAPI.Models.Validators;

public class ItineraryRequestValidator
    : AbstractValidator<ItineraryRequest>
{
    public ItineraryRequestValidator()
    {
        RuleFor(x => x.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Days)
            .GreaterThan(0)
            .LessThanOrEqualTo(14);

        RuleFor(x => x.Budget)
            .GreaterThan(0)
            .LessThanOrEqualTo(50000);
    }
}
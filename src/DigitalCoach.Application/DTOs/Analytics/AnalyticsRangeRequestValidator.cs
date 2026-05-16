using FluentValidation;

namespace DigitalCoach.Application.DTOs.Analytics;

public sealed class AnalyticsRangeRequestValidator : AbstractValidator<AnalyticsRangeRequest>
{
    public AnalyticsRangeRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value <= x.To.Value)
            .WithMessage("'from' must be earlier than or equal to 'to'.");
    }
}

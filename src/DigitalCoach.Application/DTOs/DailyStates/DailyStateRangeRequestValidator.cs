using FluentValidation;

namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed class DailyStateRangeRequestValidator : AbstractValidator<DailyStateRangeRequest>
{
    public DailyStateRangeRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From must be earlier than or equal to To.");
    }
}

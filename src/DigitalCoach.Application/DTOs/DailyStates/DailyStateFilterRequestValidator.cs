using DigitalCoach.Domain.Constants;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed class DailyStateFilterRequestValidator : AbstractValidator<DailyStateFilterRequest>
{
    public DailyStateFilterRequestValidator()
    {
        RuleFor(x => x.Mood).InclusiveBetween(1, 5).When(x => x.Mood.HasValue);
        RuleFor(x => x.Stress).InclusiveBetween(1, 5).When(x => x.Stress.HasValue);
        RuleFor(x => x.Energy).InclusiveBetween(1, 5).When(x => x.Energy.HasValue);
        RuleFor(x => x.PhysicalState).InclusiveBetween(1, 5).When(x => x.PhysicalState.HasValue);

        RuleFor(x => x.ActivityType)
            .Must(x => x is null || DailyStateActivityTypes.All.Contains(x))
            .WithMessage($"ActivityType must be one of: {string.Join(", ", DailyStateActivityTypes.All)}.");

        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From must be earlier than or equal to To.");
    }
}

using FluentValidation;

namespace DigitalCoach.Application.DTOs.Habits;

public sealed class HabitLogFilterRequestValidator : AbstractValidator<HabitLogFilterRequest>
{
    public HabitLogFilterRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From must be earlier than or equal to To.");
    }
}

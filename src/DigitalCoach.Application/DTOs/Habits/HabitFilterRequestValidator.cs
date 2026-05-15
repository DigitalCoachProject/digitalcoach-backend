using DigitalCoach.Domain.Constants;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Habits;

public sealed class HabitFilterRequestValidator : AbstractValidator<HabitFilterRequest>
{
    public HabitFilterRequestValidator()
    {
        RuleFor(x => x.Type)
            .Must(x => x is null || HabitTypes.All.Contains(x))
            .WithMessage($"Type must be one of: {string.Join(", ", HabitTypes.All)}.");

        RuleFor(x => x)
            .Must(x => x.StartDateFrom is null || x.StartDateTo is null || x.StartDateFrom <= x.StartDateTo)
            .WithMessage("StartDateFrom must be earlier than or equal to StartDateTo.");
    }
}

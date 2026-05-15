using DigitalCoach.Domain.Constants;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Habits;

public sealed class CreateHabitLogRequestValidator : AbstractValidator<CreateHabitLogRequest>
{
    public CreateHabitLogRequestValidator()
    {
        RuleFor(x => x.Date)
            .Must(x => x != default)
            .WithMessage("Date is required.");

        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(x => HabitLogStatuses.All.Contains(x))
            .WithMessage($"Status must be one of: {string.Join(", ", HabitLogStatuses.All)}.");

        RuleFor(x => x.Reason)
            .MaximumLength(100);

        RuleFor(x => x.Comment)
            .MaximumLength(500);
    }
}

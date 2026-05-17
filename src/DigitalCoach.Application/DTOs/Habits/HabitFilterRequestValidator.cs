using DigitalCoach.Domain.Constants;
using DigitalCoach.Application.Common;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Habits;

public sealed class HabitFilterRequestValidator : PaginationRequestValidator<HabitFilterRequest>
{
    private static readonly string[] SortFields = ["name", "created_at", "difficulty"];

    public HabitFilterRequestValidator()
    {
        RuleFor(x => x.Type)
            .Must(x => x is null || HabitTypes.All.Contains(x))
            .WithMessage($"Type must be one of: {string.Join(", ", HabitTypes.All)}.");

        RuleFor(x => x.SortBy)
            .Must(x => x is null || SortFields.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SortFields)}.");

        RuleFor(x => x)
            .Must(x => x.StartDateFrom is null || x.StartDateTo is null || x.StartDateFrom <= x.StartDateTo)
            .WithMessage("StartDateFrom must be earlier than or equal to StartDateTo.");
    }
}

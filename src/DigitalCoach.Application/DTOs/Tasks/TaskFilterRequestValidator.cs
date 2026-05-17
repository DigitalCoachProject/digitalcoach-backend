using DigitalCoach.Domain.Constants;
using DigitalCoach.Application.Common;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Tasks;

public sealed class TaskFilterRequestValidator : PaginationRequestValidator<TaskFilterRequest>
{
    private static readonly string[] SortFields = ["created_at", "planned_date", "deadline", "priority", "difficulty"];

    public TaskFilterRequestValidator()
    {
        RuleFor(x => x.Status)
            .Must(x => x is null || TaskStatuses.All.Contains(x))
            .WithMessage($"Status must be one of: {string.Join(", ", TaskStatuses.All)}.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 5)
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.SortBy)
            .Must(x => x is null || SortFields.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SortFields)}.");

        RuleFor(x => x)
            .Must(x => x.From is null || x.To is null || x.From <= x.To)
            .WithMessage("From must be earlier than or equal to To.");
    }
}

using DigitalCoach.Domain.Constants;
using FluentValidation;
using System.Linq.Expressions;

namespace DigitalCoach.Application.DTOs.Tasks;

public abstract class TaskRequestValidatorBase<T> : AbstractValidator<T>
{
    protected void ConfigureTaskRules(
        Expression<Func<T, string>> name,
        Expression<Func<T, string?>> description,
        Expression<Func<T, DateOnly>> plannedDate,
        Expression<Func<T, DateOnly?>> deadline,
        Expression<Func<T, int>> priority,
        Expression<Func<T, int>> difficulty,
        Expression<Func<T, string>> status)
    {
        var plannedDateAccessor = plannedDate.Compile();
        var deadlineAccessor = deadline.Compile();

        RuleFor(name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(description)
            .MaximumLength(500);

        RuleFor(plannedDate)
            .Must(x => x != default)
            .WithMessage("Planned date is required.");

        RuleFor(priority)
            .InclusiveBetween(1, 5);

        RuleFor(difficulty)
            .InclusiveBetween(1, 5);

        RuleFor(status)
            .NotEmpty()
            .Must(x => TaskStatuses.All.Contains(x))
            .WithMessage($"Status must be one of: {string.Join(", ", TaskStatuses.All)}.");

        RuleFor(x => x)
            .Must(x => deadlineAccessor(x) is null || deadlineAccessor(x) >= plannedDateAccessor(x))
            .WithMessage("Deadline must be later than or equal to planned date.");
    }
}

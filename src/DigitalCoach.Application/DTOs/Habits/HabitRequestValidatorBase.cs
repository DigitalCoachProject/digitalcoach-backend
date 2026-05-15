using DigitalCoach.Domain.Constants;
using FluentValidation;
using System.Linq.Expressions;

namespace DigitalCoach.Application.DTOs.Habits;

public abstract class HabitRequestValidatorBase<T> : AbstractValidator<T>
{
    protected void ConfigureHabitRules(
        Expression<Func<T, string>> name,
        Expression<Func<T, string?>> description,
        Expression<Func<T, string>> type,
        Expression<Func<T, int?>> frequency,
        Expression<Func<T, string?>> daysOfWeek,
        Expression<Func<T, int>> difficulty,
        Expression<Func<T, DateOnly>> startDate)
    {
        var typeAccessor = type.Compile();
        var frequencyAccessor = frequency.Compile();
        var daysOfWeekAccessor = daysOfWeek.Compile();

        RuleFor(name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(description)
            .MaximumLength(500);

        RuleFor(type)
            .NotEmpty()
            .Must(x => HabitTypes.All.Contains(x))
            .WithMessage($"Type must be one of: {string.Join(", ", HabitTypes.All)}.");

        RuleFor(frequency)
            .InclusiveBetween(1, 7)
            .When(x => frequencyAccessor(x).HasValue);

        RuleFor(daysOfWeek)
            .MaximumLength(100);

        RuleFor(difficulty)
            .InclusiveBetween(1, 5);

        RuleFor(startDate)
            .Must(x => x != default)
            .WithMessage("Start date is required.");

        RuleFor(x => x)
            .Must(x => typeAccessor(x) != HabitTypes.Daily || frequencyAccessor(x) is null)
            .WithMessage("Daily habits must not define frequency.")
            .Must(x => typeAccessor(x) != HabitTypes.Daily || string.IsNullOrWhiteSpace(daysOfWeekAccessor(x)))
            .WithMessage("Daily habits must not define days of week.")
            .Must(x => typeAccessor(x) != HabitTypes.Weekly || frequencyAccessor(x).HasValue)
            .WithMessage("Weekly habits must define frequency.")
            .Must(x => typeAccessor(x) != HabitTypes.Weekly || string.IsNullOrWhiteSpace(daysOfWeekAccessor(x)))
            .WithMessage("Weekly habits must not define days of week.")
            .Must(x => typeAccessor(x) != HabitTypes.SpecificDays || frequencyAccessor(x) is null)
            .WithMessage("Specific-days habits must not define frequency.")
            .Must(x => typeAccessor(x) != HabitTypes.SpecificDays || !string.IsNullOrWhiteSpace(daysOfWeekAccessor(x)))
            .WithMessage("Specific-days habits must define days of week.");
    }
}

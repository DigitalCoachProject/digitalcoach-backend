using DigitalCoach.Domain.Constants;
using FluentValidation;
using System.Linq.Expressions;

namespace DigitalCoach.Application.DTOs.DailyStates;

public abstract class DailyStateRequestValidatorBase<T> : AbstractValidator<T>
{
    protected void ConfigureDailyStateRules(
        Expression<Func<T, DateOnly>> date,
        Expression<Func<T, decimal?>> sleepDuration,
        Expression<Func<T, int?>> sleepQuality,
        Expression<Func<T, int>> energy,
        Expression<Func<T, int>> mood,
        Expression<Func<T, int>> stress,
        Expression<Func<T, int>> physicalState,
        Expression<Func<T, int?>> caloriesIntake,
        Expression<Func<T, int?>> mealsCount,
        Expression<Func<T, string?>> activity,
        Expression<Func<T, decimal?>> activityDuration,
        Expression<Func<T, decimal?>> screenTime,
        Expression<Func<T, string?>> dayType,
        Expression<Func<T, string?>> notes,
        Expression<Func<T, string?>> activityType)
    {
        var sleepDurationAccessor = sleepDuration.Compile();
        var sleepQualityAccessor = sleepQuality.Compile();
        var caloriesIntakeAccessor = caloriesIntake.Compile();
        var mealsCountAccessor = mealsCount.Compile();
        var activityDurationAccessor = activityDuration.Compile();
        var screenTimeAccessor = screenTime.Compile();

        RuleFor(date)
            .Must(x => x != default)
            .WithMessage("Date is required.");

        RuleFor(sleepDuration)
            .GreaterThanOrEqualTo(0)
            .When(x => sleepDurationAccessor(x).HasValue);

        RuleFor(sleepQuality)
            .InclusiveBetween(1, 5)
            .When(x => sleepQualityAccessor(x).HasValue);

        RuleFor(energy).InclusiveBetween(1, 5);
        RuleFor(mood).InclusiveBetween(1, 5);
        RuleFor(stress).InclusiveBetween(1, 5);
        RuleFor(physicalState).InclusiveBetween(1, 5);

        RuleFor(caloriesIntake)
            .GreaterThanOrEqualTo(0)
            .When(x => caloriesIntakeAccessor(x).HasValue);

        RuleFor(mealsCount)
            .GreaterThanOrEqualTo(0)
            .When(x => mealsCountAccessor(x).HasValue);

        RuleFor(activity)
            .MaximumLength(100);

        RuleFor(activityDuration)
            .GreaterThanOrEqualTo(0)
            .When(x => activityDurationAccessor(x).HasValue);

        RuleFor(screenTime)
            .GreaterThanOrEqualTo(0)
            .When(x => screenTimeAccessor(x).HasValue);

        RuleFor(dayType)
            .MaximumLength(30)
            .Must(x => x is null || DailyStateDayTypes.All.Contains(x))
            .WithMessage($"DayType must be one of: {string.Join(", ", DailyStateDayTypes.All)}.");

        RuleFor(notes)
            .MaximumLength(1000);

        RuleFor(activityType)
            .MaximumLength(50)
            .Must(x => x is null || DailyStateActivityTypes.All.Contains(x))
            .WithMessage($"ActivityType must be one of: {string.Join(", ", DailyStateActivityTypes.All)}.");
    }
}

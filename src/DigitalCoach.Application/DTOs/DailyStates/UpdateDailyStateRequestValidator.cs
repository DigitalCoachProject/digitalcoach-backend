namespace DigitalCoach.Application.DTOs.DailyStates;

public sealed class UpdateDailyStateRequestValidator : DailyStateRequestValidatorBase<UpdateDailyStateRequest>
{
    public UpdateDailyStateRequestValidator()
    {
        ConfigureDailyStateRules(
            x => x.Date,
            x => x.SleepDuration,
            x => x.SleepQuality,
            x => x.Energy,
            x => x.Mood,
            x => x.Stress,
            x => x.PhysicalState,
            x => x.CaloriesIntake,
            x => x.MealsCount,
            x => x.Activity,
            x => x.ActivityDuration,
            x => x.ScreenTime,
            x => x.DayType,
            x => x.Notes,
            x => x.ActivityType);
    }
}

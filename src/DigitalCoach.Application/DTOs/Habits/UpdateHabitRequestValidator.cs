namespace DigitalCoach.Application.DTOs.Habits;

public sealed class UpdateHabitRequestValidator : HabitRequestValidatorBase<UpdateHabitRequest>
{
    public UpdateHabitRequestValidator()
    {
        ConfigureHabitRules(
            x => x.Name,
            x => x.Description,
            x => x.Type,
            x => x.Frequency,
            x => x.DaysOfWeek,
            x => x.Difficulty,
            x => x.StartDate);
    }
}

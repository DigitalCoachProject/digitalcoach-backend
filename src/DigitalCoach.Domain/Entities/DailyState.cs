namespace DigitalCoach.Domain.Entities;

public sealed class DailyState
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public decimal? SleepDuration { get; set; }
    public int? SleepQuality { get; set; }
    public int Energy { get; set; }
    public int Mood { get; set; }
    public int Stress { get; set; }
    public int PhysicalState { get; set; }
    public bool HasIllness { get; set; }
    public bool HasPainOrInjury { get; set; }
    public int? CaloriesIntake { get; set; }
    public bool? HadMeals { get; set; }
    public int? MealsCount { get; set; }
    public bool? Overeating { get; set; }
    public bool? Undereating { get; set; }
    public string? Activity { get; set; }
    public decimal? ActivityDuration { get; set; }
    public bool? RestTaken { get; set; }
    public decimal? ScreenTime { get; set; }
    public bool? ScreenBeforeSleep { get; set; }
    public string? DayType { get; set; }
    public string? Notes { get; set; }
    public string? ActivityType { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}

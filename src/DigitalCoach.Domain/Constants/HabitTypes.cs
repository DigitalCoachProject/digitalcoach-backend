namespace DigitalCoach.Domain.Constants;

public static class HabitTypes
{
    public const string Daily = "daily";
    public const string Weekly = "weekly";
    public const string SpecificDays = "specific_days";

    public static readonly string[] All = [Daily, Weekly, SpecificDays];
}

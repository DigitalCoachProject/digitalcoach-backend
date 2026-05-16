namespace DigitalCoach.Domain.Constants;

public static class DailyStateDayTypes
{
    public const string Workday = "workday";
    public const string Weekend = "weekend";
    public const string Holiday = "holiday";
    public const string Vacation = "vacation";
    public const string SickDay = "sick_day";

    public static readonly string[] All =
    [
        Workday,
        Weekend,
        Holiday,
        Vacation,
        SickDay
    ];
}

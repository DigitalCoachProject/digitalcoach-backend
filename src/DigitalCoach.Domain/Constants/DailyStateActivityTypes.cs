namespace DigitalCoach.Domain.Constants;

public static class DailyStateActivityTypes
{
    public const string Walking = "walking";
    public const string Running = "running";
    public const string Gym = "gym";
    public const string Cycling = "cycling";
    public const string Stretching = "stretching";
    public const string Yoga = "yoga";
    public const string Other = "other";

    public static readonly string[] All =
    [
        Walking,
        Running,
        Gym,
        Cycling,
        Stretching,
        Yoga,
        Other
    ];
}

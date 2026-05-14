namespace DigitalCoach.Domain.Constants;

public static class TaskStatuses
{
    public const string Planned = "planned";
    public const string Completed = "completed";
    public const string Overdue = "overdue";
    public const string Cancelled = "cancelled";

    public static readonly string[] All = [Planned, Completed, Overdue, Cancelled];
}

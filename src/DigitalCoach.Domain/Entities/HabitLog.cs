namespace DigitalCoach.Domain.Entities;

public sealed class HabitLog
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateOnly Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? Comment { get; set; }

    public Habit Habit { get; set; } = null!;
}

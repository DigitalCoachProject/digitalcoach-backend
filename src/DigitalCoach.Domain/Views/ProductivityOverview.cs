namespace DigitalCoach.Domain.Views;

public sealed class ProductivityOverview
{
    public int UserId { get; set; }
    public DateOnly Date { get; set; }
    public int Energy { get; set; }
    public int Mood { get; set; }
    public int Stress { get; set; }
    public int PhysicalState { get; set; }
    public int TasksCount { get; set; }
    public int CompletedTasks { get; set; }
    public int HabitLogs { get; set; }
    public int CompletedHabits { get; set; }
}

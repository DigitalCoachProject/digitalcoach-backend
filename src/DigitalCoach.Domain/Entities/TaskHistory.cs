namespace DigitalCoach.Domain.Entities;

public sealed class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public DateTime ChangeDate { get; set; }
    public DateOnly OldDate { get; set; }
    public DateOnly NewDate { get; set; }
    public string? Reason { get; set; }

    public UserTask Task { get; set; } = null!;
}

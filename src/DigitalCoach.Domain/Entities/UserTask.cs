namespace DigitalCoach.Domain.Entities;

public sealed class UserTask
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateOnly PlannedDate { get; set; }
    public DateOnly? Deadline { get; set; }
    public int Priority { get; set; }
    public int Difficulty { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public int RescheduleCount { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public ICollection<TaskHistory> History { get; set; } = [];
}

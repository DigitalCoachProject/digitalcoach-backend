namespace DigitalCoach.Domain.Entities;

public sealed class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Type { get; set; } = string.Empty;
    public int? Frequency { get; set; }
    public string? DaysOfWeek { get; set; }
    public int Difficulty { get; set; }
    public DateOnly StartDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
    public ICollection<HabitLog> Logs { get; set; } = [];
}

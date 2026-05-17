namespace DigitalCoach.Domain.Entities;

public sealed class UserProfile
{
    public int Id { get; set; }
    public string? Gender { get; set; }
    public decimal Height { get; set; }
    public decimal Weight { get; set; }
    public DateOnly BirthDate { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Habit> Habits { get; set; } = [];
    public ICollection<UserTask> Tasks { get; set; } = [];
    public ICollection<DailyState> DailyStates { get; set; } = [];
    public ICollection<Recommendation> Recommendations { get; set; } = [];
}

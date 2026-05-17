using System.ComponentModel;
using DigitalCoach.Application.Common;

namespace DigitalCoach.Application.DTOs.Habits;

public sealed record HabitFilterRequest : PaginationRequest
{
    public string? Type { get; init; }
    public bool? IsActive { get; init; }
    public DateOnly? StartDateFrom { get; init; }
    public DateOnly? StartDateTo { get; init; }

    [DefaultValue(null)]
    public string? SortBy { get; init; }

    [DefaultValue(false)]
    public bool SortDescending { get; init; }
}

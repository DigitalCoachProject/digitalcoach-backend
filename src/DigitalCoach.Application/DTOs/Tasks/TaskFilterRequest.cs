using System.ComponentModel;
using DigitalCoach.Application.Common;

namespace DigitalCoach.Application.DTOs.Tasks;

public sealed record TaskFilterRequest : PaginationRequest
{
    public string? Status { get; init; }
    public int? Priority { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public bool? Overdue { get; init; }

    [DefaultValue(null)]
    public string? SortBy { get; init; }

    [DefaultValue(false)]
    public bool SortDescending { get; init; }
}

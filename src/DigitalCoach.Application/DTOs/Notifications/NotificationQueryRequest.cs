using System.ComponentModel;
using DigitalCoach.Application.Common;

namespace DigitalCoach.Application.DTOs.Notifications;

public sealed record NotificationQueryRequest : PaginationRequest
{
    [DefaultValue(null)]
    public string? SortBy { get; init; }

    [DefaultValue(false)]
    public bool SortDescending { get; init; }
}

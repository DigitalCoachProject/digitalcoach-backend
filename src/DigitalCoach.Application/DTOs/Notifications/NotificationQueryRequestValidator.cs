using DigitalCoach.Application.Common;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Notifications;

public sealed class NotificationQueryRequestValidator : PaginationRequestValidator<NotificationQueryRequest>
{
    private static readonly string[] SortFields = ["created_at", "priority", "is_read"];

    public NotificationQueryRequestValidator()
    {
        RuleFor(x => x.SortBy)
            .Must(x => x is null || SortFields.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SortFields)}.");
    }
}

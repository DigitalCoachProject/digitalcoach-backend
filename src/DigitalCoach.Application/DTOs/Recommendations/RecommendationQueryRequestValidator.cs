using DigitalCoach.Application.Common;
using FluentValidation;

namespace DigitalCoach.Application.DTOs.Recommendations;

public sealed class RecommendationQueryRequestValidator : PaginationRequestValidator<RecommendationQueryRequest>
{
    private static readonly string[] SortFields = ["created_at", "priority", "is_read"];

    public RecommendationQueryRequestValidator()
    {
        RuleFor(x => x.SortBy)
            .Must(x => x is null || SortFields.Contains(x))
            .WithMessage($"SortBy must be one of: {string.Join(", ", SortFields)}.");
    }
}

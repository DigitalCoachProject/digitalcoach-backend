using System.ComponentModel;
using DigitalCoach.Application.Common;

namespace DigitalCoach.Application.DTOs.Recommendations;

public sealed record RecommendationQueryRequest : PaginationRequest
{
    [DefaultValue(null)]
    public string? SortBy { get; init; }

    [DefaultValue(false)]
    public bool SortDescending { get; init; }
}

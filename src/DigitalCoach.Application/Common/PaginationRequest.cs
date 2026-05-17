using System.ComponentModel;

namespace DigitalCoach.Application.Common;

public abstract record PaginationRequest
{
    [DefaultValue(1)]
    public int Page { get; init; } = 1;

    [DefaultValue(20)]
    public int PageSize { get; init; } = 20;
}

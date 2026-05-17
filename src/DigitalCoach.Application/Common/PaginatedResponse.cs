namespace DigitalCoach.Application.Common;

public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages)
{
    public static PaginatedResponse<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        return new PaginatedResponse<T>(items, page, pageSize, totalItems, totalPages);
    }
}

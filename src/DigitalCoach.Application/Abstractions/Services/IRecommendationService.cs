using DigitalCoach.Application.Common;
using DigitalCoach.Application.DTOs.Recommendations;

namespace DigitalCoach.Application.Abstractions.Services;

public interface IRecommendationService
{
    Task<Result<IReadOnlyList<RecommendationResponse>>> ListAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<RecommendationResponse>>> GenerateAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int recommendationId, CancellationToken cancellationToken = default);
    Task<Result<RecommendationResponse>> MarkAsReadAsync(int userId, int recommendationId, CancellationToken cancellationToken = default);
}

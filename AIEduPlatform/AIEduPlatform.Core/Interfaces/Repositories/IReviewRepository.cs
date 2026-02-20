using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<Review?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
        Task<List<Review>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);
        Task<(List<Review> Items, int TotalCount)> GetPagedByCourseIdAsync(Guid courseId, int page, int pageSize, CancellationToken ct = default);
        Task<(double AverageRating, int TotalReviews, int[] Distribution)> GetCourseRatingSummaryAsync(Guid courseId, CancellationToken ct = default);
        Task<bool> HasStudentReviewedAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
    }
}

using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly AppDbContext _ctx;

        public ReviewRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<Review?> GetByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Reviews
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.CourseId == courseId, ct);
        }

        public async Task<List<Review>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Reviews
                .AsNoTracking()
                .Include(r => r.Student)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<(List<Review> Items, int TotalCount)> GetPagedByCourseIdAsync(Guid courseId, int page, int pageSize, CancellationToken ct = default)
        {
            var query = _ctx.Reviews
                .AsNoTracking()
                .Include(r => r.Student)
                .Where(r => r.CourseId == courseId)
                .OrderByDescending(r => r.CreatedAt);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(double AverageRating, int TotalReviews, int[] Distribution)> GetCourseRatingSummaryAsync(Guid courseId, CancellationToken ct = default)
        {
            var reviews = await _ctx.Reviews
                .AsNoTracking()
                .Where(r => r.CourseId == courseId)
                .Select(r => r.Rating)
                .ToListAsync(ct);

            if (reviews.Count == 0)
                return (0, 0, new int[5]);

            var distribution = new int[5];
            foreach (var rating in reviews)
            {
                if (rating >= 1 && rating <= 5)
                    distribution[rating - 1]++;
            }

            return (reviews.Average(), reviews.Count, distribution);
        }

        public async Task<bool> HasStudentReviewedAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Reviews
                .AnyAsync(r => r.StudentId == studentId && r.CourseId == courseId, ct);
        }
    }
}

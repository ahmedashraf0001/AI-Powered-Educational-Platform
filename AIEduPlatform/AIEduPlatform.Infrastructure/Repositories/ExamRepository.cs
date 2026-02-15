using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class ExamRepository : GenericRepository<Exam>, IExamRepository
    {
        private readonly AppDbContext _ctx;

        public ExamRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<Exam?> GetExamByIdAsync(
            Guid examId,
            bool includeQuestions = false,
            bool includeSubmissions = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Exams.AsNoTracking();

            if (includeQuestions)
                query = query.Include(e => e.Questions);
            if (includeSubmissions)
                query = query.Include(e => e.Submissions);

            return await query.FirstOrDefaultAsync(e => e.Id == examId, ct);
        }

        public async Task<List<Exam>> GetExamsByCourseIdAsync(
            Guid courseId,
            bool includeQuestions = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Exams.AsNoTracking().Where(e => e.CourseId == courseId);

            if (includeQuestions)
                query = query.Include(e => e.Questions);

            return await query.OrderByDescending(e => e.StartTime).ToListAsync(ct);
        }

        public async Task<List<Exam>> GetUpcomingExamsAsync(
            Guid courseId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _ctx.Exams
                .AsNoTracking()
                .Where(e => e.CourseId == courseId && e.StartTime > now)
                .OrderBy(e => e.StartTime)
                .ToListAsync(ct);
        }

        public async Task<List<Exam>> GetActiveExamsAsync(
            Guid courseId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _ctx.Exams
                .AsNoTracking()
                .Where(e => e.CourseId == courseId && e.StartTime <= now && e.EndTime >= now)
                .OrderBy(e => e.EndTime)
                .ToListAsync(ct);
        }

        public async Task<List<Exam>> GetPastExamsAsync(
            Guid courseId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _ctx.Exams
                .AsNoTracking()
                .Where(e => e.CourseId == courseId && e.EndTime < now)
                .OrderByDescending(e => e.EndTime)
                .ToListAsync(ct);
        }

        public async Task<List<Exam>> GetAvailableExamsForStudentAsync(
            Guid studentId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            // Single query using subqueries instead of 3 separate database hits
            return await _ctx.Exams
                .AsNoTracking()
                .Where(e => _ctx.Enrollments
                    .Where(en => en.StudentId == studentId)
                    .Select(en => en.CourseId)
                    .Contains(e.CourseId))
                .Where(e => e.StartTime <= now && e.EndTime >= now)
                .Where(e => !_ctx.Submissions
                    .Where(s => s.StudentId == studentId)
                    .Select(s => s.ExamId)
                    .Contains(e.Id))
                .OrderBy(e => e.EndTime)
                .ToListAsync(ct);
        }

        public async Task<bool> IsExamActiveAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _ctx.Exams
                .AnyAsync(e => e.Id == examId && e.StartTime <= now && e.EndTime >= now, ct);
        }

        public async Task<int> GetTotalPointsAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .SumAsync(q => q.Points, ct);
        }

        public async Task<bool> HasStudentSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default)
        {
            return await _ctx.Submissions
                .AnyAsync(s => s.ExamId == examId && s.StudentId == studentId, ct);
        }

        public async Task<Exam?> GetExamWithCourseAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Exams
                .AsNoTracking()
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == examId, ct);
        }

        public async Task<bool> IsUserTeacherOfExamAsync(
            Guid examId,
            Guid userId,
            CancellationToken ct = default)
        {
            return await _ctx.Exams
                .AnyAsync(e => e.Id == examId && e.Course.TeacherId == userId, ct);
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetExamsByCoursePagedAsync(
            Guid courseId,
            int page,
            int pageSize,
            bool includeQuestions = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Exams.AsNoTracking().Where(e => e.CourseId == courseId);
            if (includeQuestions)
                query = query.Include(e => e.Questions);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetActiveExamsPagedAsync(
            Guid courseId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _ctx.Exams.AsNoTracking()
                .Where(e => e.CourseId == courseId && e.StartTime <= now && e.EndTime >= now);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(e => e.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetUpcomingExamsPagedAsync(
            Guid courseId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _ctx.Exams.AsNoTracking()
                .Where(e => e.CourseId == courseId && e.StartTime > now);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(e => e.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetPastExamsPagedAsync(
            Guid courseId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _ctx.Exams.AsNoTracking()
                .Where(e => e.CourseId == courseId && e.EndTime < now);

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderByDescending(e => e.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task<(List<Exam> Items, int TotalCount)> GetAvailableExamsForStudentPagedAsync(
            Guid studentId,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var query = _ctx.Exams.AsNoTracking()
                .Where(e => _ctx.Enrollments
                    .Where(en => en.StudentId == studentId)
                    .Select(en => en.CourseId)
                    .Contains(e.CourseId))
                .Where(e => e.StartTime <= now && e.EndTime >= now)
                .Where(e => !_ctx.Submissions
                    .Where(s => s.StudentId == studentId)
                    .Select(s => s.ExamId)
                    .Contains(e.Id));

            var totalCount = await query.CountAsync(ct);
            var items = await query
                .OrderBy(e => e.EndTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return (items, totalCount);
        }
    }
}

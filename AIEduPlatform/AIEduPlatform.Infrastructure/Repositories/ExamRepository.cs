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
            var enrolledCourseIds = await _ctx.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync(ct);

            var submittedExamIds = await _ctx.Submissions
                .Where(s => s.StudentId == studentId)
                .Select(s => s.ExamId)
                .ToListAsync(ct);

            return await _ctx.Exams
                .AsNoTracking()
                .Where(e => enrolledCourseIds.Contains(e.CourseId) &&
                            e.StartTime <= now &&
                            e.EndTime >= now &&
                            !submittedExamIds.Contains(e.Id))
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
    }
}

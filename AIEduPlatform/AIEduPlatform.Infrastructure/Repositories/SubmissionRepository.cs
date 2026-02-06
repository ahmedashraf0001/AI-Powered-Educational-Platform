using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class SubmissionRepository : GenericRepository<Submission>, ISubmissionRepository
    {
        private readonly AppDbContext _ctx;

        public SubmissionRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<Submission?> GetSubmissionByIdAsync(
            Guid submissionId,
            bool includeExam = false,
            bool includeGrade = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions.AsNoTracking().AsQueryable();

            if (includeExam)
                query = query.Include(s => s.Exam);
            if (includeGrade)
                query = query.Include(s => s.Grade);

            return await query.FirstOrDefaultAsync(s => s.Id == submissionId, ct);
        }

        public async Task<Submission?> GetSubmissionByExamAndStudentAsync(
            Guid examId,
            Guid studentId,
            bool includeGrade = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions.AsNoTracking().AsQueryable();

            if (includeGrade)
                query = query.Include(s => s.Grade);

            return await query.FirstOrDefaultAsync(s => s.ExamId == examId && s.StudentId == studentId, ct);
        }

        public async Task<List<Submission>> GetSubmissionsByExamIdAsync(
            Guid examId,
            bool includeGrades = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions.AsNoTracking().Where(s => s.ExamId == examId);

            if (includeGrades)
                query = query.Include(s => s.Grade);

            return await query.OrderBy(s => s.SubmittedAt).ToListAsync(ct);
        }

        public async Task<List<Submission>> GetSubmissionsByStudentIdAsync(
            Guid studentId,
            bool includeExam = false,
            bool includeGrade = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions.AsNoTracking().Where(s => s.StudentId == studentId);

            if (includeExam)
                query = query.Include(s => s.Exam);
            if (includeGrade)
                query = query.Include(s => s.Grade);

            return await query.OrderByDescending(s => s.SubmittedAt).ToListAsync(ct);
        }

        public async Task<List<Submission>> GetSubmissionsByStudentAndCourseAsync(
            Guid studentId,
            Guid courseId,
            bool includeGrade = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions
                .AsNoTracking()
                .Include(s => s.Exam)
                .Where(s => s.StudentId == studentId && s.Exam.CourseId == courseId);

            if (includeGrade)
                query = query.Include(s => s.Grade);

            return await query.OrderByDescending(s => s.SubmittedAt).ToListAsync(ct);
        }

        public async Task<List<Submission>> GetUngradedSubmissionsAsync(
            Guid? examId = null,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions
                .AsNoTracking()
                .Include(s => s.Grade)
                .Where(s => s.Grade == null);

            if (examId.HasValue)
                query = query.Where(s => s.ExamId == examId.Value);

            return await query.OrderBy(s => s.SubmittedAt).ToListAsync(ct);
        }

        public async Task<List<Submission>> GetPendingApprovalSubmissionsAsync(
            Guid? examId = null,
            CancellationToken ct = default)
        {
            var query = _ctx.Submissions
                .AsNoTracking()
                .Include(s => s.Grade)
                .Where(s => s.Grade != null && s.Grade.IsAiGraded && !s.Grade.IsApproved);

            if (examId.HasValue)
                query = query.Where(s => s.ExamId == examId.Value);

            return await query.OrderBy(s => s.SubmittedAt).ToListAsync(ct);
        }

        public async Task<int> GetSubmissionCountAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Submissions.CountAsync(s => s.ExamId == examId, ct);
        }

        public async Task<bool> HasStudentSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default)
        {
            return await _ctx.Submissions
                .AnyAsync(s => s.ExamId == examId && s.StudentId == studentId, ct);
        }

        public async Task<SubmissionStats> GetExamStatsAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            var submissions = await _ctx.Submissions
                .AsNoTracking()
                .Include(s => s.Grade)
                .Where(s => s.ExamId == examId)
                .ToListAsync(ct);

            var gradedSubmissions = submissions.Where(s => s.Grade != null).ToList();
            var scores = gradedSubmissions.Select(s => s.Grade!.Score).ToList();

            return new SubmissionStats
            {
                TotalSubmissions = submissions.Count,
                GradedCount = gradedSubmissions.Count,
                PendingGradeCount = submissions.Count - gradedSubmissions.Count,
                AiGradedCount = gradedSubmissions.Count(s => s.Grade!.IsAiGraded),
                ApprovedCount = gradedSubmissions.Count(s => s.Grade!.IsApproved),
                AverageScore = scores.Count > 0 ? scores.Average() : null,
                HighestScore = scores.Count > 0 ? scores.Max() : null,
                LowestScore = scores.Count > 0 ? scores.Min() : null
            };
        }

        public async Task<Submission?> GetSubmissionWithExamAndCourseAsync(
            Guid submissionId,
            CancellationToken ct = default)
        {
            return await _ctx.Submissions
                .AsNoTracking()
                .Include(s => s.Exam)
                    .ThenInclude(e => e.Course)
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == submissionId, ct);
        }
    }
}

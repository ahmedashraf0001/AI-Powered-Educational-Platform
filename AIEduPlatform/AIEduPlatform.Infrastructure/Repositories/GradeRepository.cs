using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class GradeRepository : GenericRepository<Grade>, IGradeRepository
    {
        private readonly AppDbContext _ctx;

        public GradeRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<Grade?> GetGradeBySubmissionIdAsync(
            Guid submissionId,
            CancellationToken ct = default)
        {
            return await _ctx.Grades
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.SubmissionId == submissionId, ct);
        }

        public async Task<List<Grade>> GetGradesByStudentIdAsync(
            Guid studentId,
            bool includeSubmission = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .Where(g => g.Submission.StudentId == studentId);

            if (includeSubmission)
                query = query.Include(g => g.Submission).ThenInclude(s => s.Exam);

            return await query.OrderByDescending(g => g.CreatedAt).ToListAsync(ct);
        }

        public async Task<List<Grade>> GetGradesByExamIdAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .Where(g => g.Submission.ExamId == examId)
                .OrderByDescending(g => g.Score)
                .ToListAsync(ct);
        }

        public async Task<List<Grade>> GetGradesByStudentAndCourseAsync(
            Guid studentId,
            Guid courseId,
            CancellationToken ct = default)
        {
            return await _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .ThenInclude(s => s.Exam)
                .Where(g => g.Submission.StudentId == studentId && g.Submission.Exam.CourseId == courseId)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Grade>> GetPendingApprovalGradesAsync(
            Guid? examId = null,
            CancellationToken ct = default)
        {
            var query = _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .Where(g => g.IsAiGraded && !g.IsApproved);

            if (examId.HasValue)
                query = query.Where(g => g.Submission.ExamId == examId.Value);

            return await query.OrderBy(g => g.CreatedAt).ToListAsync(ct);
        }

        public async Task ApproveGradeAsync(
            Guid gradeId,
            CancellationToken ct = default)
        {
            await _ctx.Grades
                .Where(g => g.Id == gradeId)
                .ExecuteUpdateAsync(g => g
                    .SetProperty(x => x.IsApproved, true)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        }

        public async Task UpdateGradeAsync(
            Guid gradeId,
            float newScore,
            string feedback,
            CancellationToken ct = default)
        {
            await _ctx.Grades
                .Where(g => g.Id == gradeId)
                .ExecuteUpdateAsync(g => g
                    .SetProperty(x => x.Score, newScore)
                    .SetProperty(x => x.Feedback, feedback)
                    .SetProperty(x => x.IsApproved, true)
                    .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);
        }

        public async Task<StudentGradeStats> GetStudentStatsAsync(
            Guid studentId,
            Guid? courseId = null,
            CancellationToken ct = default)
        {
            var query = _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .ThenInclude(s => s.Exam)
                .ThenInclude(e => e.Questions)
                .Where(g => g.Submission.StudentId == studentId);

            if (courseId.HasValue)
                query = query.Where(g => g.Submission.Exam.CourseId == courseId.Value);

            var grades = await query.ToListAsync(ct);

            if (!grades.Any())
            {
                return new StudentGradeStats();
            }

            var totalPointsEarned = (int)grades.Sum(g => g.Score);
            var totalPointsPossible = grades.Sum(g => g.Submission.Exam.Questions?.Sum(q => q.Points) ?? 0);

            return new StudentGradeStats
            {
                TotalExamsTaken = grades.Count,
                AverageScore = grades.Average(g => g.Score),
                HighestScore = grades.Max(g => g.Score),
                LowestScore = grades.Min(g => g.Score),
                TotalPointsEarned = totalPointsEarned,
                TotalPointsPossible = totalPointsPossible,
                OverallPercentage = totalPointsPossible > 0 ? (float)totalPointsEarned / totalPointsPossible * 100 : 0
            };
        }

        public async Task<ExamGradeStats> GetExamStatsAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            var grades = await _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .Where(g => g.Submission.ExamId == examId)
                .ToListAsync(ct);

            if (!grades.Any())
            {
                return new ExamGradeStats();
            }

            var scores = grades.Select(g => g.Score).OrderBy(s => s).ToList();
            var average = scores.Average();
            var median = scores.Count % 2 == 0 ? (scores[scores.Count / 2 - 1] + scores[scores.Count / 2]) / 2 : scores[scores.Count / 2];

            var exam = await _ctx.Exams.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Id == examId, ct);
            var totalPoints = exam?.Questions?.Sum(q => q.Points) ?? 100;
            var passThreshold = totalPoints * 0.6f;
            var passRate = (float)scores.Count(s => s >= passThreshold) / scores.Count * 100;

            return new ExamGradeStats
            {
                TotalGraded = grades.Count,
                PendingApproval = grades.Count(g => g.IsAiGraded && !g.IsApproved),
                AverageScore = (float)average,
                MedianScore = median,
                HighestScore = scores.Max(),
                LowestScore = scores.Min(),
                PassRate = passRate
            };
        }

        public async Task<Dictionary<string, int>> GetGradeDistributionAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            var exam = await _ctx.Exams.Include(e => e.Questions).FirstOrDefaultAsync(e => e.Id == examId, ct);
            var totalPoints = exam?.Questions?.Sum(q => q.Points) ?? 100;

            var grades = await _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                .Where(g => g.Submission.ExamId == examId)
                .ToListAsync(ct);

            var distribution = new Dictionary<string, int>
            {
                { "A", 0 },
                { "B", 0 },
                { "C", 0 },
                { "D", 0 },
                { "F", 0 }
            };

            foreach (var grade in grades)
            {
                var percentage = grade.Score / totalPoints * 100;
                if (percentage >= 90) distribution["A"]++;
                else if (percentage >= 80) distribution["B"]++;
                else if (percentage >= 70) distribution["C"]++;
                else if (percentage >= 60) distribution["D"]++;
                else distribution["F"]++;
            }

            return distribution;
        }
    }
}

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
            // Build base query for grades
            var gradesQuery = _ctx.Grades
                .AsNoTracking()
                .Where(g => g.Submission.StudentId == studentId);

            if (courseId.HasValue)
                gradesQuery = gradesQuery.Where(g => g.Submission.Exam.CourseId == courseId.Value);

            // Aggregate grade stats directly in database
            var gradeStats = await gradesQuery
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    Average = g.Average(x => x.Score),
                    Max = g.Max(x => x.Score),
                    Min = g.Min(x => x.Score),
                    TotalEarned = g.Sum(x => x.Score)
                })
                .FirstOrDefaultAsync(ct);

            if (gradeStats == null || gradeStats.Count == 0)
            {
                return new StudentGradeStats();
            }

            // Get total points possible in a separate query (avoiding expensive ThenInclude)
            var examIdsQuery = _ctx.Submissions
                .Where(s => s.StudentId == studentId && s.Grade != null);

            if (courseId.HasValue)
                examIdsQuery = examIdsQuery.Where(s => s.Exam.CourseId == courseId.Value);

            var totalPointsPossible = await examIdsQuery
                .Select(s => s.ExamId)
                .Distinct()
                .Join(
                    _ctx.Questions.GroupBy(q => q.ExamId).Select(g => new { ExamId = g.Key, TotalPoints = g.Sum(q => q.Points) }),
                    examId => examId,
                    ep => ep.ExamId,
                    (examId, ep) => ep.TotalPoints)
                .SumAsync(ct);

            return new StudentGradeStats
            {
                TotalExamsTaken = gradeStats.Count,
                AverageScore = gradeStats.Average,
                HighestScore = gradeStats.Max,
                LowestScore = gradeStats.Min,
                TotalPointsEarned = (int)gradeStats.TotalEarned,
                TotalPointsPossible = totalPointsPossible,
                OverallPercentage = totalPointsPossible > 0 ? (float)gradeStats.TotalEarned / totalPointsPossible * 100 : 0
            };
        }

        public async Task<ExamGradeStats> GetExamStatsAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            // Single query to get total points for exam
            var totalPoints = await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .SumAsync(q => (int?)q.Points, ct) ?? 100;

            // Aggregate grades directly in database instead of loading all into memory
            var gradeStats = await _ctx.Grades
                .AsNoTracking()
                .Where(g => g.Submission.ExamId == examId)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Count = g.Count(),
                    Average = g.Average(x => x.Score),
                    Max = g.Max(x => x.Score),
                    Min = g.Min(x => x.Score),
                    PendingApproval = g.Count(x => x.IsAiGraded && !x.IsApproved),
                    Scores = g.Select(x => x.Score).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (gradeStats == null || gradeStats.Count == 0)
            {
                return new ExamGradeStats();
            }

            var scores = gradeStats.Scores.OrderBy(s => s).ToList();
            var median = scores.Count % 2 == 0 
                ? (scores[scores.Count / 2 - 1] + scores[scores.Count / 2]) / 2 
                : scores[scores.Count / 2];

            var passThreshold = totalPoints * 0.6f;
            var passRate = (float)scores.Count(s => s >= passThreshold) / scores.Count * 100;

            return new ExamGradeStats
            {
                TotalGraded = gradeStats.Count,
                PendingApproval = gradeStats.PendingApproval,
                AverageScore = (float)gradeStats.Average,
                MedianScore = median,
                HighestScore = gradeStats.Max,
                LowestScore = gradeStats.Min,
                PassRate = passRate
            };
        }

        public async Task<Dictionary<string, int>> GetGradeDistributionAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            // Get total points without loading questions collection
            var totalPoints = await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .SumAsync(q => (int?)q.Points, ct) ?? 100;

            // Calculate distribution directly in database
            var scores = await _ctx.Grades
                .AsNoTracking()
                .Where(g => g.Submission.ExamId == examId)
                .Select(g => g.Score)
                .ToListAsync(ct);

            var distribution = new Dictionary<string, int>
            {
                { "A", 0 },
                { "B", 0 },
                { "C", 0 },
                { "D", 0 },
                { "F", 0 }
            };

            foreach (var score in scores)
            {
                var percentage = score / totalPoints * 100;
                if (percentage >= 90) distribution["A"]++;
                else if (percentage >= 80) distribution["B"]++;
                else if (percentage >= 70) distribution["C"]++;
                else if (percentage >= 60) distribution["D"]++;
                else distribution["F"]++;
            }

            return distribution;
        }

        public async Task<Grade?> GetGradeWithSubmissionExamAndCourseAsync(
            Guid gradeId,
            CancellationToken ct = default)
        {
            return await _ctx.Grades
                .AsNoTracking()
                .Include(g => g.Submission)
                    .ThenInclude(s => s.Exam)
                        .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(g => g.Id == gradeId, ct);
        }
    }
}

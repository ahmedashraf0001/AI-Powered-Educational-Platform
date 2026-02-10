using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<User?> GetUserByIdAsync(
            Guid userId,
            bool includeEnrollments = false,
            bool includeTaughtCourses = false,
            CancellationToken ct = default)
        {
            var query = _ctx.Users.AsQueryable();

            if (includeEnrollments)
                query = query.Include(u => u.Enrollments).ThenInclude(e => e.Course);

            if (includeTaughtCourses)
                query = query.Include(u => u.TaughtCourses);

            return await query.FirstOrDefaultAsync(u => u.Id == userId, ct);
        }

        public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            return await _ctx.Users
                .FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task<List<User>> GetStudentsByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                .Select(e => e.Student)
                .ToListAsync(ct);
        }

        public async Task<List<User>> GetAllTeachersAsync(CancellationToken ct = default)
        {
            return await _ctx.Users
                .Where(u => u.TaughtCourses.Any())
                .ToListAsync(ct);
        }

        public async Task<List<User>> GetAllStudentsAsync(CancellationToken ct = default)
        {
            return await _ctx.Users
                .Where(u => u.Enrollments.Any())
                .ToListAsync(ct);
        }

        public async Task<List<User>> SearchUsersAsync(
            string searchTerm,
            int maxResults = 20,
            CancellationToken ct = default)
        {
            var term = searchTerm.ToLower();
            return await _ctx.Users
                .Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Email!.ToLower().Contains(term) ||
                    u.UserName!.ToLower().Contains(term))
                .Take(maxResults)
                .ToListAsync(ct);
        }

        public async Task<UserProfileStats> GetUserStatsAsync(Guid userId, CancellationToken ct = default)
        {
            var enrolledCount = await _ctx.Enrollments
                .CountAsync(e => e.StudentId == userId, ct);

            var completedCount = await _ctx.Enrollments
                .CountAsync(e => e.StudentId == userId && e.Status == EnrollmentStatus.Completed, ct);

            var taughtCount = await _ctx.Courses
                .CountAsync(c => c.TeacherId == userId, ct);

            var sessionCount = await _ctx.StudySessions
                .CountAsync(s => s.StudentId == userId, ct);

            var examsTaken = await _ctx.Submissions
                .CountAsync(s => s.StudentId == userId, ct);

            var avgScore = await _ctx.Submissions
                .Where(s => s.StudentId == userId && s.Grade != null)
                .Select(s => (float?)s.Grade!.Score)
                .AverageAsync(ct) ?? 0f;

            var flashcardsCreated = await _ctx.Flashcards
                .CountAsync(f => f.Session.StudentId == userId, ct);

            var quizzesTaken = await _ctx.GeneratedQuizzes
                .CountAsync(q => q.Session.StudentId == userId, ct);

            var sessionData = await _ctx.StudySessions
                .Where(s => s.StudentId == userId)
                .Select(s => new { s.StartedAt, s.LastActivity })
                .ToListAsync(ct);

            var totalStudyTime = sessionData
                .Aggregate(TimeSpan.Zero, (sum, s) => sum + (s.LastActivity - s.StartedAt));

            var lastActive = sessionData.Count > 0
                ? sessionData.Max(s => s.LastActivity)
                : DateTime.UtcNow;

            return new UserProfileStats
            {
                CoursesEnrolled = enrolledCount,
                CoursesCompleted = completedCount,
                CoursesTaught = taughtCount,
                TotalStudySessions = sessionCount,
                ExamsTaken = examsTaken,
                AverageExamScore = avgScore,
                FlashcardsCreated = flashcardsCreated,
                QuizzesTaken = quizzesTaken,
                TotalStudyTime = totalStudyTime,
                LastActiveDate = lastActive
            };
        }

        public async Task<bool> IsEnrolledInCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Enrollments
                .AnyAsync(e => e.StudentId == userId && e.CourseId == courseId, ct);
        }

        public async Task<bool> IsTeacherOfCourseAsync(Guid userId, Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Courses
                .AnyAsync(c => c.Id == courseId && c.TeacherId == userId, ct);
        }

        public async Task<List<User>> GetRecentlyActiveUsersAsync(
            int days = 7,
            int maxResults = 50,
            CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await _ctx.StudySessions
                .Where(s => s.LastActivity >= cutoff)
                .Select(s => s.Student)
                .Distinct()
                .Take(maxResults)
                .ToListAsync(ct);
        }
    }
}

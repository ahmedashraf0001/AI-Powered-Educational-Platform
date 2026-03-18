using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class StudySessionRepository : GenericRepository<StudySession>, IStudySessionRepository
    {
        private readonly AppDbContext _ctx;

        public StudySessionRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<StudySession?> GetActiveSessionAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow.AddHours(-2);

            return await _ctx.StudySessions
                .Where(s => s.StudentId == studentId
                    && s.CourseId == courseId
                    && s.EndedAt == null
                    && s.LastActivity >= cutoff)
                .OrderByDescending(s => s.LastActivity)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<List<StudySession>> GetInactiveSessionsAsync(TimeSpan inactiveDuration, CancellationToken ct = default)
        {
            var cutoff = DateTime.UtcNow - inactiveDuration;

            return await _ctx.StudySessions
                .Where(s => s.LastActivity < cutoff)
                .OrderBy(s => s.LastActivity)
                .ToListAsync(ct);
        }

        public async Task<StudySession?> GetSessionByIdAsync(
            Guid sessionId,
            bool includeMessages = false,
            bool includeFlashcards = false,
            bool includeQuizzes = false,
            bool includeMindMaps = false,
            CancellationToken ct = default)
        {
            var query = _ctx.StudySessions.AsQueryable();

            if (includeMessages)
                query = query.Include(s => s.ChatMessages);
            if (includeFlashcards)
                query = query.Include(s => s.Flashcards);
            if (includeQuizzes)
                query = query.Include(s => s.GeneratedQuizzes);
            if (includeMindMaps)
                query = query.Include(s => s.MindMaps);

            return await query.FirstOrDefaultAsync(s => s.Id == sessionId, ct);
        }

        public async Task<List<StudySession>> GetSessionsByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.StudySessions
                .AsNoTracking()
                .Where(s => s.StudentId == studentId && s.CourseId == courseId)
                .OrderByDescending(s => s.StartedAt)
                .ToListAsync(ct);
        }

        public async Task<List<StudySession>> GetSessionsByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        {
            return await _ctx.StudySessions
                .AsNoTracking()
                .Include(s => s.Course)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.LastActivity)
                .ToListAsync(ct);
        }

        public async Task<StudentSessionStats> GetStudentStatsAsync(Guid studentId, Guid? courseId = null, CancellationToken ct = default)
        {
            var query = _ctx.StudySessions
                .AsNoTracking()
                .Where(s => s.StudentId == studentId);

            if (courseId.HasValue)
                query = query.Where(s => s.CourseId == courseId.Value);

            var sessionIds = await query.Select(s => s.Id).ToListAsync(ct);

            if (sessionIds.Count == 0)
            {
                return new StudentSessionStats();
            }

            var totalMessages = await _ctx.ChatMessages
                .CountAsync(m => sessionIds.Contains(m.SessionId), ct);

            var totalFlashcards = await _ctx.Flashcards
                .CountAsync(f => sessionIds.Contains(f.SessionId), ct);

            var totalQuizzes = await _ctx.GeneratedQuizzes
                .CountAsync(q => sessionIds.Contains(q.SessionId), ct);

            var totalMindMaps = await _ctx.MindMaps
                .CountAsync(m => sessionIds.Contains(m.SessionId), ct);

            var sessions = await query
                .Select(s => new { s.StartedAt, s.LastActivity })
                .ToListAsync(ct);

            var totalStudyTime = TimeSpan.FromTicks(
                sessions.Sum(s => (s.LastActivity - s.StartedAt).Ticks));

            var lastSessionDate = sessions
                .Max(s => (DateTime?)s.LastActivity);

            return new StudentSessionStats
            {
                TotalSessions = sessionIds.Count,
                TotalMessages = totalMessages,
                TotalFlashcards = totalFlashcards,
                TotalQuizzes = totalQuizzes,
                TotalMindMaps = totalMindMaps,
                TotalStudyTime = totalStudyTime,
                LastSessionDate = lastSessionDate
            };
        }

        public async Task UpdateLastActivityAsync(Guid sessionId, CancellationToken ct = default)
        {
            await _ctx.StudySessions
                .Where(s => s.Id == sessionId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastActivity, DateTime.UtcNow), ct);
        }
    }
}

using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IStudySessionRepository : IGenericRepository<StudySession>
    {
        Task<StudySession?> GetActiveSessionAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
        Task<List<StudySession>> GetInactiveSessionsAsync(TimeSpan inactiveDuration, CancellationToken ct = default);
        Task<StudySession?> GetSessionByIdAsync(Guid sessionId, bool includeMessages = false, bool includeFlashcards = false, bool includeQuizzes = false, bool includeMindMaps = false, CancellationToken ct = default);
        Task<List<StudySession>> GetSessionsByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
        Task<List<StudySession>> GetSessionsByStudentIdAsync(Guid studentId, CancellationToken ct = default);
        Task<StudentSessionStats> GetStudentStatsAsync(Guid studentId, Guid? courseId = null, CancellationToken ct = default);
        Task UpdateLastActivityAsync(Guid sessionId, CancellationToken ct = default);
    }
}
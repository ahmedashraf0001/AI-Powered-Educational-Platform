using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class StudySessionRepository : GenericRepository<StudySession>, IStudySessionRepository
    {
        public StudySessionRepository(AppDbContext context) : base(context)
        {
        }

        public Task<StudySession?> GetActiveSessionAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudySession>> GetInactiveSessionsAsync(TimeSpan inactiveDuration, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<StudySession?> GetSessionByIdAsync(Guid sessionId, bool includeMessages = false, bool includeFlashcards = false, bool includeQuizzes = false, bool includeMindMaps = false, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudySession>> GetSessionsByStudentAndCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<StudySession>> GetSessionsByStudentIdAsync(Guid studentId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<StudentSessionStats> GetStudentStatsAsync(Guid studentId, Guid? courseId = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateLastActivityAsync(Guid sessionId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}

using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IGeneratedQuizRepository : IGenericRepository<GeneratedQuiz>
    {
        Task<List<GeneratedQuiz>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
        Task<GeneratedQuiz?> GetByIdWithSessionAsync(Guid quizId, CancellationToken ct = default);
    }
}

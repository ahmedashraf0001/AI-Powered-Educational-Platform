using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IFlashcardRepository : IGenericRepository<Flashcard>
    {
        Task<List<Flashcard>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
        Task<List<Flashcard>> GetDueForReviewAsync(Guid sessionId, CancellationToken ct = default);
    }
}

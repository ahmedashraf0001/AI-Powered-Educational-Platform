using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IChatMessageRepository : IGenericRepository<ChatMessage>
    {
        Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
        Task<List<ChatMessage>> GetRecentBySessionIdAsync(Guid sessionId, int count, CancellationToken ct = default);
        Task<int> GetCountBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    }
}

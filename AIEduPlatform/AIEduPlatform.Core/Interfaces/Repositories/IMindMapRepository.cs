using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IMindMapRepository : IGenericRepository<MindMap>
    {
        Task<List<MindMap>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default);
    }
}

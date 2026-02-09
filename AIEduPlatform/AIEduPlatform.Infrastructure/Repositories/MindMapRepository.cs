using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class MindMapRepository : GenericRepository<MindMap>, IMindMapRepository
    {
        private readonly AppDbContext _ctx;

        public MindMapRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<MindMap>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _ctx.MindMaps
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync(ct);
        }
    }
}

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
    public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
    {
        private readonly AppDbContext _ctx;

        public ChatMessageRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<ChatMessage>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _ctx.ChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<ChatMessage>> GetRecentBySessionIdAsync(Guid sessionId, int count, CancellationToken ct = default)
        {
            return await _ctx.ChatMessages
                .AsNoTracking()
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.CreatedAt)
                .Take(count)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<int> GetCountBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _ctx.ChatMessages
                .CountAsync(m => m.SessionId == sessionId, ct);
        }
    }
}

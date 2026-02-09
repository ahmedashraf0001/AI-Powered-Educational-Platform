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
    public class FlashcardRepository : GenericRepository<Flashcard>, IFlashcardRepository
    {
        private readonly AppDbContext _ctx;

        public FlashcardRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<Flashcard>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _ctx.Flashcards
                .AsNoTracking()
                .Where(f => f.SessionId == sessionId)
                .OrderBy(f => f.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<List<Flashcard>> GetDueForReviewAsync(Guid sessionId, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            return await _ctx.Flashcards
                .Where(f => f.SessionId == sessionId && f.NextReview <= now)
                .OrderBy(f => f.NextReview)
                .ToListAsync(ct);
        }
    }
}

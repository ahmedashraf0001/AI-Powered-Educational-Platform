using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class GeneratedQuizRepository : GenericRepository<GeneratedQuiz>, IGeneratedQuizRepository
    {
        private readonly AppDbContext _ctx;

        public GeneratedQuizRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<GeneratedQuiz>> GetBySessionIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            return await _ctx.GeneratedQuizzes
                .AsNoTracking()
                .Where(q => q.SessionId == sessionId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task<GeneratedQuiz?> GetByIdWithSessionAsync(Guid quizId, CancellationToken ct = default)
        {
            return await _ctx.GeneratedQuizzes
                .Include(q => q.Session)
                .FirstOrDefaultAsync(q => q.Id == quizId, ct);
        }
    }
}

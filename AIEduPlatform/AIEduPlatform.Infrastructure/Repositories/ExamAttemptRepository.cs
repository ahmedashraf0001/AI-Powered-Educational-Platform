using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class ExamAttemptRepository : GenericRepository<ExamAttempt>, IExamAttemptRepository
    {
        private readonly AppDbContext _ctx;

        public ExamAttemptRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<ExamAttempt?> GetByExamAndStudentAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default)
        {
            return await _ctx.ExamAttempts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId, ct);
        }

        public async Task<ExamAttempt> GetOrCreateAttemptAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default)
        {
            var existing = await _ctx.ExamAttempts
                .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId, ct);

            if (existing != null)
                return existing;

            var attempt = new ExamAttempt
            {
                ExamId = examId,
                StudentId = studentId,
                StartedAt = DateTime.UtcNow,
                IsSubmitted = false
            };

            await _ctx.ExamAttempts.AddAsync(attempt, ct);
            await _ctx.SaveChangesAsync(ct);

            return attempt;
        }

        public async Task MarkAsSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default)
        {
            var attempt = await _ctx.ExamAttempts
                .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId, ct);

            if (attempt != null)
            {
                attempt.IsSubmitted = true;
                await _ctx.SaveChangesAsync(ct);
            }
        }

        public async Task SaveAnswersAsync(
            Guid examId,
            Guid studentId,
            string answersJson,
            CancellationToken ct = default)
        {
            var attempt = await _ctx.ExamAttempts
                .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId, ct);

            if (attempt != null)
            {
                attempt.SavedAnswers = answersJson;
                attempt.UpdatedAt = DateTime.UtcNow;
                await _ctx.SaveChangesAsync(ct);
            }
        }
    }
}

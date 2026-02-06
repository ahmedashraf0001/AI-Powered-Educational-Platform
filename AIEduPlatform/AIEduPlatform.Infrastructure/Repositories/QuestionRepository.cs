using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class QuestionRepository : GenericRepository<Question>, IQuestionRepository
    {
        private readonly AppDbContext _ctx;

        public QuestionRepository(AppDbContext ctx) : base(ctx)
        {
            _ctx = ctx;
        }

        public async Task<List<Question>> GetQuestionsByExamIdAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .AsNoTracking()
                .Where(q => q.ExamId == examId)
                .OrderBy(q => q.Order)
                .ToListAsync(ct);
        }

        public async Task<List<Question>> GetQuestionsByTypeAsync(
            Guid examId,
            QuestionType type,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .AsNoTracking()
                .Where(q => q.ExamId == examId && q.Type == type)
                .OrderBy(q => q.Order)
                .ToListAsync(ct);
        }

        public async Task<int> GetTotalPointsForExamAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .SumAsync(q => q.Points, ct);
        }

        public async Task AddQuestionsToExamAsync(
            Guid examId,
            List<Question> questions,
            CancellationToken ct = default)
        {
            var maxOrder = await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .MaxAsync(q => (int?)q.Order, ct) ?? -1;

            for (int i = 0; i < questions.Count; i++)
            {
                questions[i].ExamId = examId;
                questions[i].Order = maxOrder + i + 1;
            }
            await _ctx.Questions.AddRangeAsync(questions, ct);
        }

        public async Task<int> DeleteQuestionsByExamIdAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .ExecuteDeleteAsync(ct);
        }

        public async Task ReorderQuestionsAsync(Guid examId, Dictionary<Guid, int> questionOrders, CancellationToken ct = default)
        {
            var questionIds = questionOrders.Keys.ToList();

            var questions = await _ctx.Questions
                .Where(q => q.ExamId == examId && questionIds.Contains(q.Id))
                .ToListAsync(ct);

            foreach (var question in questions)
            {
                if (questionOrders.TryGetValue(question.Id, out var order))
                {
                    question.Order = order;
                }
            }
        }

        public async Task<int> GetMaxOrderForExamAsync(Guid examId, CancellationToken ct = default)
        {
            return await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .MaxAsync(q => (int?)q.Order, ct) ?? 0;
        }

        public async Task<Dictionary<QuestionType, int>> GetQuestionCountByTypeAsync(
            Guid examId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .Where(q => q.ExamId == examId)
                .GroupBy(q => q.Type)
                .ToDictionaryAsync(g => g.Key, g => g.Count(), ct);
        }

        public async Task<Question?> GetQuestionWithExamAndCourseAsync(
            Guid questionId,
            CancellationToken ct = default)
        {
            return await _ctx.Questions
                .AsNoTracking()
                .Include(q => q.Exam)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(q => q.Id == questionId, ct);
        }
    }
}

using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddBulkQuestions
{
    public record AddBulkQuestionsCommand : IRequest<List<Guid>>
    {
        public Guid ExamId { get; init; }
        public List<BulkQuestionItem> Questions { get; init; } = [];
    }

    public record BulkQuestionItem
    {
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }
}

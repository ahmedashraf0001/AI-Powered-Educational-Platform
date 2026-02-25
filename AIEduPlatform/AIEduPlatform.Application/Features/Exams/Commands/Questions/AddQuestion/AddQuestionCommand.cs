using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddQuestion
{
    public record AddQuestionCommand : IRequest<Guid>
    {
        public Guid ExamId { get; init; }
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public List<string> Options { get; init; } = [];
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }
}

using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.UpdateQuestion
{
    public record UpdateQuestionCommand : IRequest<Unit>
    {
        public Guid QuestionId { get; init; }
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
    }
}

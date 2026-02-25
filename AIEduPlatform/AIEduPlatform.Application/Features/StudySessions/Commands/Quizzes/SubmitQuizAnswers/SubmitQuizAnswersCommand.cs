using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.SubmitQuizAnswers
{
    public record SubmitQuizAnswersCommand : IRequest<QuizResultDto>
    {
        public Guid SessionId { get; init; }
        public Guid QuizId { get; init; }
        public Dictionary<int, string> Answers { get; init; } = new();
    }
}

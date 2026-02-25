using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.GenerateQuiz
{
    public record GenerateQuizCommand : IRequest<GeneratedQuizDto>
    {
        public Guid SessionId { get; init; }
        public string Topic { get; init; } = string.Empty;
        public int NumberOfQuestions { get; init; } = 5;
        public string Difficulty { get; init; } = "medium";
        public List<string> QuestionTypes { get; init; } = new() { "mcq" };
        public List<Guid>? LectureIds { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }
}

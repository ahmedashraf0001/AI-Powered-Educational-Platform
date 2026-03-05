using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionQuiz
{
    public record GenerateSectionQuizCommand : IRequest<GeneratedQuizDto>
    {
        public Guid SessionId { get; init; }
        public Guid SectionId { get; init; }
        public int NumberOfQuestions { get; init; } = 5;
        public string Difficulty { get; init; } = "medium";
        public List<string> QuestionTypes { get; init; } = new() { "mcq" };
    }
}

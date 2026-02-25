using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.DeleteQuestion
{
    public record DeleteQuestionCommand : IRequest<Unit>
    {
        public Guid QuestionId { get; init; }
    }
}

using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.ReorderQuestions
{
    public record ReorderQuestionsCommand : IRequest<Unit>
    {
        public Guid ExamId { get; init; }
        public Dictionary<Guid, int> QuestionOrders { get; init; } = [];
    }
}

using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Questions.GetExamQuestions
{
    public record GetExamQuestionsQuery : IRequest<List<QuestionDto>>
    {
        public Guid ExamId { get; init; }
    }
}

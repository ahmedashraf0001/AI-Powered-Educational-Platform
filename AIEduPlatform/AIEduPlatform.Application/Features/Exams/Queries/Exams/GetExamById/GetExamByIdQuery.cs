using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamById
{
    public record GetExamByIdQuery : IRequest<ExamDetailDto>
    {
        public Guid ExamId { get; init; }
        public bool IncludeQuestions { get; init; } = true;
    }
}

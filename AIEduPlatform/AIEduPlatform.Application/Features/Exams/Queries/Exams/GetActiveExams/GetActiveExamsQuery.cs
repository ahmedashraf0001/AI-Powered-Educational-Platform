using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetActiveExams
{
    public record GetActiveExamsQuery : IRequest<List<ExamDto>>
    {
        public Guid CourseId { get; init; }
    }
}

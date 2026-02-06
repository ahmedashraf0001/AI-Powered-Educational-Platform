using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetUpcomingExams
{
    public record GetUpcomingExamsQuery : IRequest<List<ExamDto>>
    {
        public Guid CourseId { get; init; }
    }
}

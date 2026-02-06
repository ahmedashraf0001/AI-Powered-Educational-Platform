using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamsByCourse
{
    public record GetExamsByCourseQuery : IRequest<List<ExamDto>>
    {
        public Guid CourseId { get; init; }
    }
}

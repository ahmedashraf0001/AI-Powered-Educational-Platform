using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetUpcomingExams
{
    public record GetUpcomingExamsQuery : IRequest<PagedResult<ExamDto>>
    {
        public Guid CourseId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

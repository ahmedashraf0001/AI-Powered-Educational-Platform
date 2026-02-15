using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetAvailableExamsForStudent
{
    public record GetAvailableExamsForStudentQuery : IRequest<PagedResult<ExamDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetUngradedSubmissions
{
    public record GetUngradedSubmissionsQuery : IRequest<PagedResult<SubmissionDto>>
    {
        public Guid? ExamId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

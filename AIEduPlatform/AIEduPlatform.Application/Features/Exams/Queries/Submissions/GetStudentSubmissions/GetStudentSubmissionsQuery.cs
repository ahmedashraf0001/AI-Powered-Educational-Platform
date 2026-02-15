using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions
{
    public record GetStudentSubmissionsQuery : IRequest<PagedResult<SubmissionDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetSubmissionById
{
    public record GetSubmissionByIdQuery : IRequest<SubmissionDetailDto>
    {
        public Guid SubmissionId { get; init; }
    }
}

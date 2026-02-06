using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetUngradedSubmissions
{
    public record GetUngradedSubmissionsQuery : IRequest<List<SubmissionDto>>
    {
        public Guid? ExamId { get; init; }
    }
}

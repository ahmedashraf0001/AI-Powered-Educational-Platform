using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissions
{
    public record GetExamSubmissionsQuery : IRequest<List<SubmissionDto>>
    {
        public Guid ExamId { get; init; }
    }
}

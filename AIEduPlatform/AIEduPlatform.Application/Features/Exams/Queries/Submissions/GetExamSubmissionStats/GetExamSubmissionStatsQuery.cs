using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissionStats
{
    public record GetExamSubmissionStatsQuery : IRequest<SubmissionStats>
    {
        public Guid ExamId { get; init; }
    }
}

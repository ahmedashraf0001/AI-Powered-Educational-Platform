using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeBySubmission
{
    public record GetGradeBySubmissionQuery : IRequest<GradeDto>
    {
        public Guid SubmissionId { get; init; }
    }
}

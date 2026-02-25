using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeDistribution
{
    public record GetGradeDistributionQuery : IRequest<Dictionary<string, int>>
    {
        public Guid ExamId { get; init; }
    }
}

using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamTotalPoints
{
    public record GetExamTotalPointsQuery : IRequest<int>
    {
        public Guid ExamId { get; init; }
    }
}

using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes
{
    public record GetSessionQuizzesQuery : IRequest<List<GeneratedQuizDto>>
    {
        public Guid SessionId { get; init; }
    }
}

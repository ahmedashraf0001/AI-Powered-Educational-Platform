using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes
{
    public record GetSessionQuizzesQuery : IRequest<PagedResult<GeneratedQuizDto>>
    {
        public Guid SessionId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}

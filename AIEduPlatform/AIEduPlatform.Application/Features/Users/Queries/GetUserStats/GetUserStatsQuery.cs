using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetUserStats
{
    public record GetUserStatsQuery : IRequest<UserProfileStats>
    {
        public Guid? UserId { get; init; }
    }
}

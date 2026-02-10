using AIEduPlatform.Core.DTOs.Users;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetUserProfile
{
    public record GetUserProfileQuery : IRequest<UserProfileDto>
    {
        public Guid UserId { get; init; }
    }
}

using AIEduPlatform.Core.DTOs.Users;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Queries.GetMyProfile
{
    public record GetMyProfileQuery : IRequest<UserProfileDto>;
}

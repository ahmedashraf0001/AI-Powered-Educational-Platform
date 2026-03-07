using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Users;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ICurrentUserService _currentUser;

        public GetMyProfileQueryHandler(
            UserManager<UserEntity> userManager,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user);

            return new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                Website = user.Website,
                LinkedInUrl = user.LinkedInUrl,
                Location = user.Location,
            };
        }
    }
}

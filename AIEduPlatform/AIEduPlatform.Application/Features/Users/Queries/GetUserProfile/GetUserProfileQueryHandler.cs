using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Users;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Queries.GetUserProfile
{
    public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
    {
        private readonly UserManager<UserEntity> _userManager;

        public GetUserProfileQueryHandler(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString())
                ?? throw new NotFoundException("User", request.UserId);

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

using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Commands.BecomeTeacher
{
    public class BecomeTeacherCommandHandler : IRequestHandler<BecomeTeacherCommand, AuthResponseDto>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IAuthTokenService _authTokenService;

        public BecomeTeacherCommandHandler(
            UserManager<UserEntity> userManager,
            ICurrentUserService currentUser,
            IAuthTokenService authTokenService)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _authTokenService = authTokenService;
        }

        public async Task<AuthResponseDto> Handle(BecomeTeacherCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("Teacher"))
                throw new BadRequestException("You are already a teacher.");

            var result = await _userManager.AddToRoleAsync(user, "Teacher");

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to add teacher role: {errors}");
            }

            return await _authTokenService.GenerateAuthTokensAsync(user);
        }
    }
}

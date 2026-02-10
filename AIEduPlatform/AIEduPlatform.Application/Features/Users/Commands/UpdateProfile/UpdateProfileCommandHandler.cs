using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ICurrentUserService _currentUser;

        public UpdateProfileCommandHandler(
            UserManager<UserEntity> userManager,
            ICurrentUserService currentUser)
        {
            _userManager = userManager;
            _currentUser = currentUser;
        }

        public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            if (request.FirstName is not null)
                user.FirstName = request.FirstName;

            if (request.LastName is not null)
                user.LastName = request.LastName;

            if (request.UserName is not null)
            {
                var existing = await _userManager.FindByNameAsync(request.UserName);
                if (existing is not null && existing.Id != user.Id)
                    throw new BadRequestException("Username is already taken.");

                user.UserName = request.UserName;
            }

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to update profile: {errors}");
            }

            return Unit.Value;
        }
    }
}

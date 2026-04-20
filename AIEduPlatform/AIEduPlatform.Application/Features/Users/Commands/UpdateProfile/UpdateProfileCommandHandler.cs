using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ICurrentUserService _currentUser;
        private readonly IFileService _fileService;
        private readonly ILogger<UpdateProfileCommandHandler> _logger;

        public UpdateProfileCommandHandler(
            UserManager<UserEntity> userManager,
            ICurrentUserService currentUser,
            IFileService fileService,
            ILogger<UpdateProfileCommandHandler> logger)
        {
            _userManager = userManager;
            _currentUser = currentUser;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.UserName))
            {
                var existing = await _userManager.FindByNameAsync(request.UserName);
                if (existing is not null && existing.Id != user.Id)
                    throw new BadRequestException("Username is already taken.");

                user.UserName = request.UserName;
            }

            if (!string.IsNullOrEmpty(request.Bio))
                user.Bio = request.Bio;

            if (!string.IsNullOrEmpty(request.Website))
                user.Website = request.Website;

            if (!string.IsNullOrEmpty(request.LinkedInUrl))
                user.LinkedInUrl = request.LinkedInUrl;

            if (!string.IsNullOrEmpty(request.Location))
                user.Location = request.Location;
                
            if (!string.IsNullOrEmpty(request.Qualifications))
                user.Qualifications = request.Qualifications;

            if (!string.IsNullOrEmpty(request.ExpertiseAreas))
                user.ExpertiseAreas = request.ExpertiseAreas;

            // Avatar URL manual override (only if not empty)
            if (!string.IsNullOrEmpty(request.AvatarUrl))
                user.AvatarUrl = request.AvatarUrl;

            // Handle avatar removal
            if (request.RemoveAvatar && !string.IsNullOrEmpty(user.AvatarUrl))
            {
                await _fileService.DeleteFileAsync(user.AvatarUrl, cancellationToken);
                user.AvatarUrl = null;
            }
            else if (request.AvatarStream != null && !string.IsNullOrEmpty(request.AvatarFileName))
            {
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                    await _fileService.DeleteFileAsync(user.AvatarUrl, cancellationToken);

                var uploadResult = await _fileService.UploadFileAsync(
                    request.AvatarStream,
                    request.AvatarFileName,
                    request.AvatarContentType ?? "image/jpeg",
                    "avatars",
                    cancellationToken);

                if (uploadResult.Success)
                    user.AvatarUrl = uploadResult.FileUrl;
                else
                    _logger.LogWarning("Failed to upload avatar: {Error}", uploadResult.ErrorMessage);
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

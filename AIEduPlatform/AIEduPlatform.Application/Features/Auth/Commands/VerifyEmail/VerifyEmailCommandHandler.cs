using AIEduPlatform.Application.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.VerifyEmail
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly ILogger<VerifyEmailCommandHandler> _logger;

        public VerifyEmailCommandHandler(
            UserManager<UserEntity> userManager,
            ILogger<VerifyEmailCommandHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Unit> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new NotFoundException("User", request.Email);

            if (user.IsEmailVerified)
                throw new BadRequestException("Email is already verified.");

            if (user.EmailVerificationToken != request.Token)
                throw new BadRequestException("Invalid verification token.");

            if (user.EmailVerificationTokenExpiry.HasValue && user.EmailVerificationTokenExpiry.Value < DateTime.UtcNow)
                throw new BadRequestException("Verification token has expired. Please request a new one.");

            user.IsEmailVerified = true;
            user.EmailConfirmed = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Email verification failed: {errors}");
            }

            _logger.LogInformation("Email verified for user: {Email}", request.Email);

            return Unit.Value;
        }
    }
}

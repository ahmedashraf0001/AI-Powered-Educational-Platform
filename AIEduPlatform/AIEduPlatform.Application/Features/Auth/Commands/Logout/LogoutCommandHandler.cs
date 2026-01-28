using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LogoutCommandHandler(
            UserManager<UserEntity> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(request.UserId, out var userGuid))
            {
                throw new BadRequestException("Invalid user ID format.");
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            
            if (user == null)
            {
                throw new NotFoundException("User", request.UserId);
            }

            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (refreshToken == null)
            {
                throw new NotFoundException("Refresh token not found.");
            }

            if (refreshToken.UserId != userGuid)
            {
                throw new BadRequestException("Token does not belong to user.");
            }

            await _refreshTokenRepository.RevokeTokenAsync(request.RefreshToken);

            return Unit.Value;
        }
    }
}

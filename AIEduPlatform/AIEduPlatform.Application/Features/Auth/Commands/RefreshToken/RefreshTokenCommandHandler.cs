using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenResponseDto>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public RefreshTokenCommandHandler(
            UserManager<UserEntity> userManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<TokenResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);

            if (principal == null)
            {
                throw new BadRequestException("Invalid access token.");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new BadRequestException("Invalid token claims.");
            }

            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (storedRefreshToken == null)
            {
                throw new BadRequestException("Invalid refresh token.");
            }

            if (storedRefreshToken.IsRevoked)
            {
                throw new BadRequestException("Refresh token has been revoked.");
            }

            if (storedRefreshToken.ExpiryTime < DateTime.UtcNow)
            {
                throw new BadRequestException("Refresh token has expired.");
            }

            if (storedRefreshToken.UserId.ToString() != userId)
            {
                throw new BadRequestException("Token does not belong to user.");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User", userId);
            }

            var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

            storedRefreshToken.Token = newRefreshToken;
            storedRefreshToken.ExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays);
            _refreshTokenRepository.Update(storedRefreshToken);
            await _refreshTokenRepository.SaveAsync();

            return new TokenResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}

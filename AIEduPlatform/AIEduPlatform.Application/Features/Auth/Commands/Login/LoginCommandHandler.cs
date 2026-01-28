using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using RefreshTokenEntity = AIEduPlatform.Core.Domain.Entities.RefreshToken;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public LoginCommandHandler(
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

        public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var result = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!result)
            {
                throw new BadRequestException("Invalid email or password.");
            }

            var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var jwtSettings = _configuration.GetSection("JwtSettings");
            var refreshTokenExpiryDays = int.Parse(jwtSettings["RefreshTokenExpiryDays"] ?? "7");

            var refreshTokenEntity = new RefreshTokenEntity
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = DateTime.UtcNow.AddDays(refreshTokenExpiryDays),
                IsRevoked = false
            };
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["AccessTokenExpiryMinutes"] ?? "60")),
                RefreshTokenExpiration = refreshTokenEntity.ExpiryTime
            };
        }
    }
}

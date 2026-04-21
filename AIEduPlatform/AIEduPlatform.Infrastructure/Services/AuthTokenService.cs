using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Auth;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace AIEduPlatform.Infrastructure.Services
{
    public class AuthTokenService : IAuthTokenService
    {
        private const string PrimaryJwtSectionName = "JWT";
        private const string LegacyJwtSectionName = "JwtSettings";
        private const int DefaultAccessTokenExpiryMinutes = 60;
        private const int DefaultRefreshTokenExpiryDays = 7;

        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public AuthTokenService(
            IJwtTokenGenerator jwtTokenGenerator,
            IRefreshTokenRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> GenerateAuthTokensAsync(User user)
        {
            var accessToken = await _jwtTokenGenerator.GenerateAccessTokenAsync(user);
            var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

            var jwtSettings = GetJwtSettingsSection();
            var refreshTokenExpiryDays = GetPositiveIntSetting(
                jwtSettings,
                "RefreshTokenExpiryDays",
                DefaultRefreshTokenExpiryDays);
            var accessTokenExpiryMinutes = GetPositiveIntSetting(
                jwtSettings,
                "AccessTokenExpiryMinutes",
                DefaultAccessTokenExpiryMinutes);
            var now = DateTime.UtcNow;

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryTime = now.AddDays(refreshTokenExpiryDays),
                IsRevoked = false
            };
            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            await _refreshTokenRepository.SaveAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiration = now.AddMinutes(accessTokenExpiryMinutes),
                RefreshTokenExpiration = refreshTokenEntity.ExpiryTime
            };
        }

        private IConfigurationSection GetJwtSettingsSection()
        {
            var primary = _configuration.GetSection(PrimaryJwtSectionName);
            return primary.Exists() ? primary : _configuration.GetSection(LegacyJwtSectionName);
        }

        private static int GetPositiveIntSetting(IConfigurationSection section, string key, int defaultValue)
        {
            return int.TryParse(section[key], out var value) && value > 0
                ? value
                : defaultValue;
        }
    }
}

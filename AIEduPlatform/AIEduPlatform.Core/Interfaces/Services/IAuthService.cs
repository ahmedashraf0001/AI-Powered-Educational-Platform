using AIEduPlatform.Core.DTOs.Auth;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task RegisterUserAsync(RegisterDto registerDto);
        Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);
        Task RevokeTokenAsync(string userId);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto model);
        string GeneratePasswordResetLink(string frontendResetPasswordUrlBase, string token, string userEmail);
        Task ResetPasswordAsync(ResetPasswordRequestDto model);
    }
}

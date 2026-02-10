using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Auth;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IAuthTokenService
    {
        Task<AuthResponseDto> GenerateAuthTokensAsync(User user);
    }
}

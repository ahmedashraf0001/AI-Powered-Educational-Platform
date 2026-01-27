using AIEduPlatform.Core.DTOs.Auth;
using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.RefreshToken
{
    public record RefreshTokenCommand : IRequest<TokenResponseDto>
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}

using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.Logout
{
    public record LogoutCommand : IRequest<Unit>
    {
        public string UserId { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}

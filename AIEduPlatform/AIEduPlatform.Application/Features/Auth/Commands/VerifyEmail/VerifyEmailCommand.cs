using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.VerifyEmail
{
    public record VerifyEmailCommand : IRequest<Unit>
    {
        public string Token { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
    }
}

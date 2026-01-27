using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.Register
{
    public record RegisterCommand : IRequest<Unit>
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
    }
}

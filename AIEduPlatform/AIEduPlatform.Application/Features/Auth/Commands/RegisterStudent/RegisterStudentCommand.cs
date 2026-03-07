using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.RegisterStudent
{
    public record RegisterStudentCommand : IRequest<Unit>
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
    }
}

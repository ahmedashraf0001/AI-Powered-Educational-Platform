using MediatR;

namespace AIEduPlatform.Application.Features.Auth.Commands.RegisterTeacher
{
    public record RegisterTeacherCommand : IRequest<Unit>
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Bio { get; init; } = string.Empty;
        public string Qualifications { get; init; } = string.Empty;
        public string Subjects { get; init; } = string.Empty;
    }
}

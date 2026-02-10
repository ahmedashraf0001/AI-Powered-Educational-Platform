using AIEduPlatform.Core.DTOs.Auth;
using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.BecomeTeacher
{
    public record BecomeTeacherCommand : IRequest<AuthResponseDto>;
}

using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand : IRequest<Unit>
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? UserName { get; init; }
    }
}

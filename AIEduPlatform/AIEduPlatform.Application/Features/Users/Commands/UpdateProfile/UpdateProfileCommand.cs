using MediatR;

namespace AIEduPlatform.Application.Features.Users.Commands.UpdateProfile
{
    public record UpdateProfileCommand : IRequest<Unit>
    {
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public string? UserName { get; init; }
        public string? Bio { get; init; }
        public string? Qualifications { get; init; }
        public string? Subjects { get; init; }
        public string? GradeLevel { get; init; }
        public string? Interests { get; init; }
        public string? AvatarUrl { get; init; }
        public Stream? AvatarStream { get; init; }
        public string? AvatarFileName { get; init; }
        public string? AvatarContentType { get; init; }
        public bool RemoveAvatar { get; init; }
        public string? Website { get; init; }
        public string? LinkedInUrl { get; init; }
        public string? Title { get; init; }
        public string? Location { get; init; }
        public string? ExpertiseAreas { get; init; }
    }
}

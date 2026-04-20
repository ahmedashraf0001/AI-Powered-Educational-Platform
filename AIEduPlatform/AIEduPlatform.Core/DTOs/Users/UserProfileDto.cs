namespace AIEduPlatform.Core.DTOs.Users
{
    public record UserProfileDto
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public string? FirstName { get; init; }
        public string? LastName { get; init; }
        public List<string> Roles { get; init; } = [];
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        public string? Bio { get; init; }
        public string? AvatarUrl { get; init; }
        public string? Website { get; init; }
        public string? LinkedInUrl { get; init; }
        public string? Location { get; init; }
        public string? Qualifications { get; init; }
        public string? ExpertiseAreas { get; init; }
    }
}

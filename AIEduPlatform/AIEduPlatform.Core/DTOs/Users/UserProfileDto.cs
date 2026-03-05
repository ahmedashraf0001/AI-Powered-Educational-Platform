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

        // Teacher-specific fields
        public string? Bio { get; init; }
        public string? Qualifications { get; init; }
        public string? Subjects { get; init; }

        // Student-specific fields
        public string? GradeLevel { get; init; }
        public string? Interests { get; init; }

        // Extended profile fields
        public string? AvatarUrl { get; init; }
        public string? Website { get; init; }
        public string? LinkedInUrl { get; init; }
        public string? Title { get; init; }
        public string? Location { get; init; }
        public List<string> ExpertiseAreas { get; init; } = [];
    }
}

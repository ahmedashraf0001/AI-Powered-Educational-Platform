using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Email verification
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationTokenExpiry { get; set; }
        public bool IsEmailVerified { get; set; }

        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Website { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? Location { get; set; }

        public ICollection<Course> TaughtCourses { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Review> Reviews { get; set; }

        // Voice / audio preferences for dialogue generation
        public UserVoiceSettings? VoiceSettings { get; set; }
    }
}

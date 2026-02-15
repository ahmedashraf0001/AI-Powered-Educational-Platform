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

        public ICollection<Course> TaughtCourses { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Submission> Submissions { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}

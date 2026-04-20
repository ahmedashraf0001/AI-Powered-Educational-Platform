using System;
using System.Collections.Generic;
using System.Text;
using Pgvector;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid TeacherId { get; set; }
        public bool IsPublished { get; set; }
        public decimal Price { get; set; }
        public int CurrentEnrollmentCount { get; set; }
        public string? ThumbnailUrl { get; set; }

        public User Teacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Lecture> Lectures { get; set; }
        public ICollection<Exam> Exams { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<CourseCategory> CourseCategories { get; set; }
        public ICollection<CourseTag> CourseTags { get; set; }
        public Vector? TagEmbedding { get; set; }

        // Tag Rebuild Tracking
        public bool NeedsTagRebuild { get; set; }
        public int PendingContentChanges { get; set; }
        public DateTime? LastTagUpdatedAt { get; set; }
        public bool HasContentDeletions { get; set; }
    }
    public class CourseIncludeOptions
    {
        public bool IncludeEnrollments { get; set; } = false;
        public bool IncludeLectures { get; set; } = false;
        public bool IncludeTags { get; set; } = false;
        public bool IncludeExams { get; set; } = false;
        public bool IncludeStudySessions { get; set; } = false;
        public bool IncludeMaterials { get; set; } = false;
        public bool IncludeTeacher { get; set; } = false;
        public bool IncludeReviews { get; set; } = false;
        public bool IncludeCategories { get; set; } = false;
        public bool IncludeCourseTags { get; set; } = false;
    }
}




using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public Guid TeacherId { get; set; }
        public bool IsPublished { get; set; }

        public User Teacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; }
        public ICollection<Lecture> Lectures { get; set; }
        public ICollection<Exam> Exams { get; set; }
        public ICollection<StudySession> StudySessions { get; set; }
    }
    public class CourseIncludeOptions
    {
        public bool IncludeEnrollments { get; set; } = false;
        public bool IncludeLectures { get; set; } = false;
        public bool IncludeExams { get; set; } = false;
        public bool IncludeStudySessions { get; set; } = false;

        public bool IncludeMaterials { get; set; } = false;

    }
}

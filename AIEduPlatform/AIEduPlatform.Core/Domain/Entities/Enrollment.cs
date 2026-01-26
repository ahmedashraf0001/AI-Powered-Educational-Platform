using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Enrollment : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime EnrolledAt { get; set; }
        public EnrollmentStatus Status { get; set; }

        public User Student { get; set; }
        public Course Course { get; set; }
    }
}

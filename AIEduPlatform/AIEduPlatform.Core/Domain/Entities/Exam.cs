using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }

        public Course Course { get; set; }
        public ICollection<Question> Questions { get; set; }
        public ICollection<Submission> Submissions { get; set; }
    }
}

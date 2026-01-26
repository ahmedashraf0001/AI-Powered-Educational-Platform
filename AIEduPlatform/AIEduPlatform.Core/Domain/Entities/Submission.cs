using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Submission : BaseEntity
    {
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public string Answers { get; set; } // JSON: Student's answers to exam questions
        public DateTime SubmittedAt { get; set; }

        public Exam Exam { get; set; }
        public User Student { get; set; }
        public Grade Grade { get; set; }
    }
}

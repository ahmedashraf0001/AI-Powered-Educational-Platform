using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Grade : BaseEntity
    {
        public Guid SubmissionId { get; set; }
        public float Score { get; set; }
        public string Feedback { get; set; }
        public bool IsAiGraded { get; set; }
        public bool IsApproved { get; set; }
        public string? QuestionResults { get; set; }

        public Submission Submission { get; set; }
    }
}

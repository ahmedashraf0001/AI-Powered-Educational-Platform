using System;

namespace AIEduPlatform.Core.Domain.Entities
{
    /// <summary>
    /// Tracks when a student starts an exam attempt for timer persistence.
    /// </summary>
    public class ExamAttempt : BaseEntity
    {
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsSubmitted { get; set; }

        // Stores the current saved answers as JSON (for resume functionality)
        public string? SavedAnswers { get; set; }

        // Navigation properties
        public Exam Exam { get; set; }
        public User Student { get; set; }
    }
}

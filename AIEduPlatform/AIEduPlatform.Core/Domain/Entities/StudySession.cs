using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class StudySession : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid CourseId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsActive => EndedAt == null;

        public User Student { get; set; }
        public Course Course { get; set; }
        public ICollection<ChatMessage> ChatMessages { get; set; }
        public ICollection<GeneratedQuiz> GeneratedQuizzes { get; set; }
        public ICollection<Flashcard> Flashcards { get; set; }
        public ICollection<MindMap> MindMaps { get; set; }
    }
}

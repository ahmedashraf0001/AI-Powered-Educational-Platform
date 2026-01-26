using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class GeneratedQuiz : BaseEntity
    {
        public Guid SessionId { get; set; }
        public string Topic { get; set; }
        public QuizDifficulty Difficulty { get; set; }
        public string Questions { get; set; } // JSON: Array of quiz questions
        public string StudentAnswers { get; set; } // JSON: Array of student answers
        public float Score { get; set; }

        public StudySession Session { get; set; }
    }
}

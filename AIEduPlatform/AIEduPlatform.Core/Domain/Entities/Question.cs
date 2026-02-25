using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Question : BaseEntity
    {
        public Guid ExamId { get; set; }
        public QuestionType Type { get; set; }
        public string Text { get; set; }
        public string Options { get; set; } // JSON: Array of answer options
        public string CorrectAnswer { get; set; }
        public int Points { get; set; }
        public int Order { get; set; }

        /// <summary>
        /// Model answer for essay/short answer questions - used by AI grading
        /// </summary>
        public string? ModelAnswer { get; set; }

        /// <summary>
        /// JSON: Grading rubric criteria for essay questions - used by AI grading
        /// </summary>
        public string? GradingCriteria { get; set; }

        public Exam Exam { get; set; }
    }
}

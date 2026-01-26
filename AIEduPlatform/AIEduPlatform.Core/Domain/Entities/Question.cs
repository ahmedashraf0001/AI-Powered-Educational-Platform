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

        public Exam Exam { get; set; }
    }
}

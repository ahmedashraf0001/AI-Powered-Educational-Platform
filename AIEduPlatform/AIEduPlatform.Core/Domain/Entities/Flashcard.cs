using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Flashcard : BaseEntity
    {
        public Guid SessionId { get; set; }
        public string Topic { get; set; }
        public string FrontText { get; set; }
        public string BackText { get; set; }
        public int ReviewCount { get; set; }
        public DateTime NextReview { get; set; }

        public StudySession Session { get; set; }
    }
}

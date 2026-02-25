using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class ChatMessage : BaseEntity
    {
        public Guid SessionId { get; set; }
        public ChatRole Role { get; set; }
        public string Content { get; set; }
        public string? Sources { get; set; } // JSON: Array of source references
        public StudySession Session { get; set; }
    }
}

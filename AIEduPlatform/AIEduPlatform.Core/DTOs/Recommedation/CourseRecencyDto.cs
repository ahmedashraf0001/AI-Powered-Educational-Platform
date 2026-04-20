using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Recommedation
{
    public class CourseRecencyDto
    {
        public Guid CourseId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastTagUpdatedAt { get; set; }
    }
}

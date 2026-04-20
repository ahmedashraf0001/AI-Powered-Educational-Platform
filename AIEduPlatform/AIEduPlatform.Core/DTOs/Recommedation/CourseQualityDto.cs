using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Recommedation
{
    public class CourseQualityDto
    {
        public Guid CourseId { get; set; }
        public double AverageRating { get; set; }   // 0–5
        public int ReviewCount { get; set; }
        public double CompletionRate { get; set; }  // 0–1
    }
}

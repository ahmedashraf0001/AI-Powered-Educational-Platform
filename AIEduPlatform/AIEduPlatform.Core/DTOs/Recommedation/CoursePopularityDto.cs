using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Recommedation
{
    public class CoursePopularityDto
    {
        public Guid CourseId { get; set; }
        public int EnrollmentCount { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class CourseTag
    {
        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public Guid TagId { get; set; }
        public Tag Tag { get; set; }
    }
}

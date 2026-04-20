using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Tags
{
    public class CourseTagsDto
    {
        public Guid CourseId { get; set; }
        public List<Guid> TagIds { get; set; }
    }
}

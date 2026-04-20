using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; }        
        public string DisplayName { get; set; } 

        public ICollection<CourseTag> CourseTags { get; set; }
        public ICollection<UserTag> UserTags { get; set; }
    }
}

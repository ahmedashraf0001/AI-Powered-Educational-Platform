using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Lecture : BaseEntity
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrderIndex { get; set; }

        public Course Course { get; set; }
        public ICollection<Material> Materials { get; set; }
    }
}

using AIEduPlatform.Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Material : BaseEntity
    {
        public Guid LectureId { get; set; }
        public MaterialType Type { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public string Transcript { get; set; }
        public string Summary { get; set; }

        public Lecture Lecture { get; set; }
    }
}

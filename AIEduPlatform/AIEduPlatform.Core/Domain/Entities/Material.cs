using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Material : BaseEntity
    {
        public Guid LectureId { get; set; }
        public MaterialType Type { get; set; }
        public string Title { get; set; }
        public string FileUrl { get; set; }
        public string? Summary { get; set; }
        public bool Indexed { get; set; } = false;
        public Lecture Lecture { get; set; }
        public ICollection<MaterialChunk> Chunks { get; set; }
    }
}

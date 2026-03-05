using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class MaterialProgress : BaseEntity
    {
        public Guid StudentId { get; set; }
        public Guid MaterialId { get; set; }
        public int LastPosition { get; set; }
        public bool IsCompleted { get; set; }

        public User Student { get; set; } = null!;
        public Material Material { get; set; } = null!;
    }
}

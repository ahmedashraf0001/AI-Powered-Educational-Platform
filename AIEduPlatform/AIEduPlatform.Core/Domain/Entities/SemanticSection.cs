using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class SemanticSection : BaseEntity
    {
        public Guid MaterialId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;

        // For Video/Audio: time in seconds
        public int? StartSeconds { get; set; }
        public int? EndSeconds { get; set; }

        // For PDF: page ranges
        public int? StartPage { get; set; }
        public int? EndPage { get; set; }

        public int OrderIndex { get; set; }

        public Material Material { get; set; } = null!;
    }
}

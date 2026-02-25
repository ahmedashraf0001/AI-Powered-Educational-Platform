using Pgvector;
using System;
using System.Collections.Generic;

using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class MaterialChunk:BaseEntity
    {
        public Guid MaterialId { get; set; }
        public Material Material { get; set; }

        public string Content { get; set; } = string.Empty;

        public Vector Embedding { get; set; } = default!;
        public Dictionary<string, object>? AdditionalData { get; set; }
        public ICollection<ConceptChunkMap> ConceptMappings { get; set; }
        public string? Section { get; set; }
        public string? LectureName { get; set; }
        public string? CourseName { get; set; }
        public string? PageOrTimestamp { get; set; }
    }
}

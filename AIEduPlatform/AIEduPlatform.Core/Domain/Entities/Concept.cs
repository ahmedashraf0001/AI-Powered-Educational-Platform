using Pgvector;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Concept : BaseEntity
    {
        public string Name { get; set; }
        public string NormalizedName { get; set; }
        public string Type { get; set; }
        public string Summary { get; set; }
        public Vector Embedding { get; set; }

        public Guid CourseId { get; set; }
        public Course Course { get; set; }

        public Guid MaterialId { get; set; }
        public Material Material { get; set; }

        // Graph edges
        public ICollection<ConceptRelation> OutgoingRelations { get; set; }
        public ICollection<ConceptRelation> IncomingRelations { get; set; }

        // Evidence mapping
        public ICollection<ConceptChunkMap> ConceptChunks { get; set; }
    }
    public class ConceptRelation : BaseEntity
    {
        public Guid FromConceptId { get; set; }
        public Concept FromConcept { get; set; }

        public Guid ToConceptId { get; set; }
        public Concept ToConcept { get; set; }

        public string RelationType { get; set; }
    }

    public class ConceptChunkMap : BaseEntity
    {
        public Guid ConceptId { get; set; }
        public Concept Concept { get; set; }

        public Guid ChunkId { get; set; }
        public MaterialChunk Chunk { get; set; }
    }
}

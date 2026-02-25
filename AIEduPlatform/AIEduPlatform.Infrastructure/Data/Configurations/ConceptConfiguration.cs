using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class ConceptEntityConfiguration : IEntityTypeConfiguration<Concept>
    {
        public void Configure(EntityTypeBuilder<Concept> builder)
        {
            builder.Property(c => c.Embedding)
                .HasColumnType("vector(384)"); // match your embedding dimension

            builder.HasIndex(c => c.NormalizedName);
            builder.HasIndex(c => c.CourseId);

            // Composite index for concept lookup within a course
            builder.HasIndex(c => new { c.CourseId, c.NormalizedName }).IsUnique();

            builder.HasOne(c => c.Course)
                .WithMany()
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // delete concepts when course deleted

            builder.HasOne(c => c.Material)
                .WithMany()
                .HasForeignKey(c => c.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    public class ConceptRelationConfiguration : IEntityTypeConfiguration<ConceptRelation>
    {
        public void Configure(EntityTypeBuilder<ConceptRelation> builder)
        {
            builder
                .HasOne(cr => cr.FromConcept)
                .WithMany(c => c.OutgoingRelations)
                .HasForeignKey(cr => cr.FromConceptId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(cr => cr.ToConcept)
                .WithMany(c => c.IncomingRelations)
                .HasForeignKey(cr => cr.ToConceptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    public class ConceptChunkMapConfiguration : IEntityTypeConfiguration<ConceptChunkMap>
    {
        public void Configure(EntityTypeBuilder<ConceptChunkMap> builder)
        {
            builder.HasIndex(cc => cc.ChunkId);
            builder.HasIndex(cc => cc.ConceptId); 
            builder.HasIndex(cc => new { cc.ConceptId, cc.ChunkId }).IsUnique();

            builder
                .HasOne(cc => cc.Concept)
                .WithMany(c => c.ConceptChunks)
                .HasForeignKey(cc => cc.ConceptId)
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .HasOne(cc => cc.Chunk)
                .WithMany(c => c.ConceptMappings)
                .HasForeignKey(cc => cc.ChunkId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    } 
}

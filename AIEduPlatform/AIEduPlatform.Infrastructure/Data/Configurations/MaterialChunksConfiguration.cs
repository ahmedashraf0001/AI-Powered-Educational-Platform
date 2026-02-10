using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class MaterialChunksConfiguration : IEntityTypeConfiguration<MaterialChunk>
    {
        public void Configure(EntityTypeBuilder<MaterialChunk> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.MaterialId)
                .IsRequired();

            builder.Property(x => x.Embedding)
                .HasColumnType("vector(384)")
                .IsRequired();

            builder.Property(m => m.Content)
                .HasColumnType("text");

            builder.Property(m => m.Section)
                .HasColumnType("text");

            builder.Property(m => m.LectureName)
                .HasColumnType("text");

            builder.Property(m => m.CourseName)
                .HasColumnType("text");

            builder.Property(m => m.PageOrTimestamp)
                .HasColumnType("text");

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            builder.Property(m => m.AdditionalData)
                .HasColumnType("jsonb");

            // Relationships
            builder.HasOne(m => m.Material)
                .WithMany(l => l.Chunks)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(m => m.MaterialId);
            builder.HasIndex(m => m.Section);
            builder.HasIndex(m => m.Content)
               .HasMethod("GIN")
               .HasOperators("gin_trgm_ops");

        }
    }
}

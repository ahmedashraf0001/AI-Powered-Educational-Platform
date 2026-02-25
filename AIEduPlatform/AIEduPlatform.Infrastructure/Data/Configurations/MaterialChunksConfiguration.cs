using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

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

            builder.Property(e => e.AdditionalData)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null)
                )
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                    (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions)null),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, (JsonSerializerOptions)null).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(v, (JsonSerializerOptions)null), (JsonSerializerOptions)null)!
                ));

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

using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class MaterialConfiguration : IEntityTypeConfiguration<Material>
    {
        public void Configure(EntityTypeBuilder<Material> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.LectureId)
                .IsRequired();

            builder.Property(m => m.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(m => m.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.FileUrl)
                .HasMaxLength(500);

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(m => m.Lecture)
                .WithMany(l => l.Materials)
                .HasForeignKey(m => m.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(m => m.LectureId);
            builder.HasIndex(m => m.Type);
        }
    }
}

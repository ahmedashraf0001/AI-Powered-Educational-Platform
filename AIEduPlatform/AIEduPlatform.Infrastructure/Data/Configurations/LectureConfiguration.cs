using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class LectureConfiguration : IEntityTypeConfiguration<Lecture>
    {
        public void Configure(EntityTypeBuilder<Lecture> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.CourseId)
                .IsRequired();

            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(l => l.Description)
                .HasMaxLength(2000);

            builder.Property(l => l.OrderIndex)
                .IsRequired();

            builder.Property(l => l.CreatedAt)
                .IsRequired();

            builder.Property(l => l.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(l => l.Course)
                .WithMany(c => c.Lectures)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(l => l.Materials)
                .WithOne(m => m.Lecture)
                .HasForeignKey(m => m.LectureId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(l => l.CourseId);
            builder.HasIndex(l => new { l.CourseId, l.OrderIndex });
        }
    }
}

using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.StudentId)
                .IsRequired();

            builder.Property(e => e.CourseId)
                .IsRequired();

            builder.Property(e => e.EnrolledAt)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();
            builder.HasIndex(e => e.Status);
        }
    }
}

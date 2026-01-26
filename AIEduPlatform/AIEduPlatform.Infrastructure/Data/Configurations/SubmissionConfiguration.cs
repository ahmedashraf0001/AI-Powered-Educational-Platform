using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ExamId)
                .IsRequired();

            builder.Property(s => s.StudentId)
                .IsRequired();

            builder.Property(s => s.Answers)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(s => s.SubmittedAt)
                .IsRequired();

            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(s => s.Exam)
                .WithMany(e => e.Submissions)
                .HasForeignKey(s => s.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Grade)
                .WithOne(g => g.Submission)
                .HasForeignKey<Grade>(g => g.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(s => new { s.ExamId, s.StudentId })
                .IsUnique();
            builder.HasIndex(s => s.SubmittedAt);
        }
    }
}

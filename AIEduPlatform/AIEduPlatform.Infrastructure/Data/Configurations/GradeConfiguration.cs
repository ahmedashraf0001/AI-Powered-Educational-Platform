using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class GradeConfiguration : IEntityTypeConfiguration<Grade>
    {
        public void Configure(EntityTypeBuilder<Grade> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.SubmissionId)
                .IsRequired();

            builder.Property(g => g.Score)
                .IsRequired()
                .HasPrecision(5, 2);

            builder.Property(g => g.Feedback)
                .HasMaxLength(2000);

            builder.Property(g => g.IsAiGraded)
                .IsRequired();

            builder.Property(g => g.IsApproved)
                .IsRequired();

            builder.Property(g => g.QuestionResults)
                .HasMaxLength(4000);

            builder.Property(g => g.CreatedAt)
                .IsRequired();

            builder.Property(g => g.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(g => g.Submission)
                .WithOne(s => s.Grade)
                .HasForeignKey<Grade>(g => g.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(g => g.SubmissionId)
                .IsUnique();
            builder.HasIndex(g => g.IsAiGraded);
        }
    }
}

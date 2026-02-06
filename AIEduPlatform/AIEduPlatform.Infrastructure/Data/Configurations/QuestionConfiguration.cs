using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class QuestionConfiguration : IEntityTypeConfiguration<Question>
    {
        public void Configure(EntityTypeBuilder<Question> builder)
        {
            builder.HasKey(q => q.Id);

            builder.Property(q => q.ExamId)
                .IsRequired();

            builder.Property(q => q.Type)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(q => q.Text)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(q => q.Options)
                .HasColumnType("jsonb");

            builder.Property(q => q.CorrectAnswer)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(q => q.Points)
                .IsRequired();

            builder.Property(q => q.Order)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(q => q.ModelAnswer)
                .HasMaxLength(5000);

            builder.Property(q => q.GradingCriteria)
                .HasColumnType("jsonb");

            builder.Property(q => q.CreatedAt)
                .IsRequired();

            builder.Property(q => q.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(q => q.Exam)
                .WithMany(e => e.Questions)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(q => q.ExamId);
            builder.HasIndex(q => q.Type);
            builder.HasIndex(q => new { q.ExamId, q.Order })
                .HasDatabaseName("IX_Questions_ExamId_Order");
        }
    }
}

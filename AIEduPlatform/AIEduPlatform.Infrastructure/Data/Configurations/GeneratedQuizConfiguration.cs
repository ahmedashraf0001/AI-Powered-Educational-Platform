using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class GeneratedQuizConfiguration : IEntityTypeConfiguration<GeneratedQuiz>
    {
        public void Configure(EntityTypeBuilder<GeneratedQuiz> builder)
        {
            builder.HasKey(gq => gq.Id);

            builder.Property(gq => gq.SessionId)
                .IsRequired();

            builder.Property(gq => gq.Topic)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(gq => gq.Difficulty)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(gq => gq.Questions)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(gq => gq.StudentAnswers)
                .HasColumnType("jsonb");

            builder.Property(gq => gq.Score)
                .HasPrecision(5, 2);

            builder.Property(gq => gq.CreatedAt)
                .IsRequired();

            builder.Property(gq => gq.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(gq => gq.Session)
                .WithMany(ss => ss.GeneratedQuizzes)
                .HasForeignKey(gq => gq.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(gq => gq.SessionId);
            builder.HasIndex(gq => gq.Topic);
        }
    }
}

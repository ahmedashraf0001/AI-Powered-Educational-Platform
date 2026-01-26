using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class StudySessionConfiguration : IEntityTypeConfiguration<StudySession>
    {
        public void Configure(EntityTypeBuilder<StudySession> builder)
        {
            builder.HasKey(ss => ss.Id);

            builder.Property(ss => ss.StudentId)
                .IsRequired();

            builder.Property(ss => ss.CourseId)
                .IsRequired();

            builder.Property(ss => ss.StartedAt)
                .IsRequired();

            builder.Property(ss => ss.LastActivity)
                .IsRequired();

            builder.Property(ss => ss.CreatedAt)
                .IsRequired();

            builder.Property(ss => ss.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(ss => ss.Student)
                .WithMany(u => u.StudySessions)
                .HasForeignKey(ss => ss.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ss => ss.Course)
                .WithMany(c => c.StudySessions)
                .HasForeignKey(ss => ss.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ss => ss.ChatMessages)
                .WithOne(cm => cm.Session)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ss => ss.GeneratedQuizzes)
                .WithOne(gq => gq.Session)
                .HasForeignKey(gq => gq.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ss => ss.Flashcards)
                .WithOne(f => f.Session)
                .HasForeignKey(f => f.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ss => ss.MindMaps)
                .WithOne(mm => mm.Session)
                .HasForeignKey(mm => mm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ss => ss.StudentId);
            builder.HasIndex(ss => ss.CourseId);
            builder.HasIndex(ss => ss.StartedAt);
        }
    }
}

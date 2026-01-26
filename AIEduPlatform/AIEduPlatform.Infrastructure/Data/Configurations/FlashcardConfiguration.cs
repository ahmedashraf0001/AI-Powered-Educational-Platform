using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
    {
        public void Configure(EntityTypeBuilder<Flashcard> builder)
        {
            builder.HasKey(f => f.Id);

            builder.Property(f => f.SessionId)
                .IsRequired();

            builder.Property(f => f.Topic)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(f => f.FrontText)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(f => f.BackText)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(f => f.ReviewCount)
                .IsRequired();

            builder.Property(f => f.NextReview)
                .IsRequired();

            builder.Property(f => f.CreatedAt)
                .IsRequired();

            builder.Property(f => f.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(f => f.Session)
                .WithMany(ss => ss.Flashcards)
                .HasForeignKey(f => f.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(f => f.SessionId);
            builder.HasIndex(f => f.NextReview);
        }
    }
}

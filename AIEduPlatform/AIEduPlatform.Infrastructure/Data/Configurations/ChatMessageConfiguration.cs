using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.HasKey(cm => cm.Id);

            builder.Property(cm => cm.SessionId)
                .IsRequired();

            builder.Property(cm => cm.Role)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(cm => cm.Content)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(cm => cm.Sources)
                .HasColumnType("jsonb");

            builder.Property(cm => cm.CreatedAt)
                .IsRequired();

            builder.Property(cm => cm.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(cm => cm.Session)
                .WithMany(ss => ss.ChatMessages)
                .HasForeignKey(cm => cm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(cm => cm.SessionId);
            builder.HasIndex(cm => cm.CreatedAt);
        }
    }
}

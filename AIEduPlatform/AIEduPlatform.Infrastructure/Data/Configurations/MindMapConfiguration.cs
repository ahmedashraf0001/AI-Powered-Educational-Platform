using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class MindMapConfiguration : IEntityTypeConfiguration<MindMap>
    {
        public void Configure(EntityTypeBuilder<MindMap> builder)
        {
            builder.HasKey(mm => mm.Id);

            builder.Property(mm => mm.SessionId)
                .IsRequired();

            builder.Property(mm => mm.Topic)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(mm => mm.Nodes)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(mm => mm.Connections)
                .IsRequired()
                .HasColumnType("jsonb");

            builder.Property(mm => mm.CreatedAt)
                .IsRequired();

            builder.Property(mm => mm.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(mm => mm.Session)
                .WithMany(ss => ss.MindMaps)
                .HasForeignKey(mm => mm.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(mm => mm.SessionId);
            builder.HasIndex(mm => mm.Topic);
        }
    }
}

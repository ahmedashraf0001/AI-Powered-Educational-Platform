using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Configurations
{
    public class SemanticSectionConfiguration : IEntityTypeConfiguration<SemanticSection>
    {
        public void Configure(EntityTypeBuilder<SemanticSection> builder)
        {
            builder.HasKey(s => s.Id);
            builder.HasIndex(s => s.MaterialId);

            builder.Property(s => s.Title).IsRequired().HasMaxLength(300);
            builder.Property(s => s.Summary).IsRequired().HasMaxLength(2000);

            builder.HasOne(s => s.Material)
                .WithMany(m => m.SemanticSections)
                .HasForeignKey(s => s.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Configurations
{
    public class MaterialProgressConfiguration : IEntityTypeConfiguration<MaterialProgress>
    {
        public void Configure(EntityTypeBuilder<MaterialProgress> builder)
        {
            builder.HasKey(mp => mp.Id);
            builder.HasIndex(mp => new { mp.StudentId, mp.MaterialId }).IsUnique();
            builder.HasIndex(mp => mp.StudentId);
            builder.HasIndex(mp => mp.MaterialId);

            builder.HasOne(mp => mp.Student)
                .WithMany()
                .HasForeignKey(mp => mp.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mp => mp.Material)
                .WithMany(m => m.ProgressRecords)
                .HasForeignKey(mp => mp.MaterialId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

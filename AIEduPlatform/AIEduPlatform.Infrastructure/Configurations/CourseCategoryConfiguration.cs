using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Configurations
{
    public class CourseCategoryConfiguration : IEntityTypeConfiguration<CourseCategory>
    {
        public void Configure(EntityTypeBuilder<CourseCategory> builder)
        {
            builder.HasKey(cc => cc.Id);
            builder.HasIndex(cc => new { cc.CourseId, cc.CategoryId }).IsUnique();

            builder.HasOne(cc => cc.Course)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cc => cc.Category)
                .WithMany(c => c.CourseCategories)
                .HasForeignKey(cc => cc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Configurations
{
    public class UserTagConfiguration : IEntityTypeConfiguration<UserTag>
    {
        public void Configure(EntityTypeBuilder<UserTag> builder)
        {
            builder.HasKey(ut => new { ut.UserId, ut.TagId });

            builder.HasOne(ut => ut.User)
                .WithMany(u => u.UserTags)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ut => ut.Tag)
                .WithMany(t => t.UserTags)
                .HasForeignKey(ut => ut.TagId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Property(ut => ut.Weight)
                .HasDefaultValue(1.0);
        }
    }
}

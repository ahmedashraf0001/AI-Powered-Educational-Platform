using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Infrastructure.Data.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.UserId)
                .IsRequired();

            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(rt => rt.ExpiryTime)
                .IsRequired();

            builder.Property(rt => rt.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(rt => rt.CreatedAt)
                .IsRequired();

            builder.Property(rt => rt.UpdatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(rt => rt.Token)
                .IsUnique();
            
            builder.HasIndex(rt => rt.UserId);
            
            builder.HasIndex(rt => rt.ExpiryTime);
            
            builder.HasIndex(rt => new { rt.UserId, rt.IsRevoked });
        }
    }
}
using AIEduPlatform.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIEduPlatform.Infrastructure.Configurations
{
    public class UserVoiceSettingsConfiguration : IEntityTypeConfiguration<UserVoiceSettings>
    {
        public void Configure(EntityTypeBuilder<UserVoiceSettings> builder)
        {
            builder.ToTable("UserVoiceSettings");

            builder.HasKey(v => v.Id);

            // One-to-one: each user has at most one voice settings row
            builder.HasIndex(v => v.UserId).IsUnique();

            builder.HasOne(v => v.User)
                .WithOne(u => u.VoiceSettings)
                .HasForeignKey<UserVoiceSettings>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(v => v.TeacherVoiceId).HasMaxLength(50).HasDefaultValue("Damien Black");
            builder.Property(v => v.StudentVoiceId).HasMaxLength(50).HasDefaultValue("Daisy Studious");
            builder.Property(v => v.TeacherSpeed).HasDefaultValue(0.95);
            builder.Property(v => v.StudentSpeed).HasDefaultValue(1.0);
            builder.Property(v => v.OutputFormat).HasMaxLength(10).HasDefaultValue("mp3");
            builder.Property(v => v.SampleRate).HasDefaultValue(24000);
            builder.Property(v => v.IncludePauses).HasDefaultValue(true);
            builder.Property(v => v.PauseDurationMs).HasDefaultValue(500);
            builder.Property(v => v.PauseMultiplier).HasDefaultValue(1.0);
            builder.Property(v => v.NormalizeAudio).HasDefaultValue(true);
        }
    }
}

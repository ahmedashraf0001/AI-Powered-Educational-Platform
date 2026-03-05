namespace AIEduPlatform.Core.Domain.Entities
{
    /// <summary>
    /// Persisted voice/audio preferences for dialogue generation.
    /// One-to-one with <see cref="User"/> — created on first save.
    /// </summary>
    public class UserVoiceSettings : BaseEntity
    {
        // ── Owner ───────────────────────────────────────────
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // ── Voice selection ─────────────────────────────────
        /// <summary>XTTS v2 voice name for the teacher speaker.</summary>
        public string TeacherVoiceId { get; set; } = "Damien Black";

        /// <summary>XTTS v2 voice name for the student speaker.</summary>
        public string StudentVoiceId { get; set; } = "Daisy Studious";

        // ── Speed ───────────────────────────────────────────
        /// <summary>Speech speed multiplier for teacher (0.5–2.0).</summary>
        public double TeacherSpeed { get; set; } = 0.95;

        /// <summary>Speech speed multiplier for student (0.5–2.0).</summary>
        public double StudentSpeed { get; set; } = 1.0;

        // ── Audio output ────────────────────────────────────
        /// <summary>Output audio format: mp3, wav, ogg.</summary>
        public string OutputFormat { get; set; } = "mp3";

        /// <summary>Audio sample rate in Hz.</summary>
        public int SampleRate { get; set; } = 24000;

        /// <summary>Whether to insert silence between dialogue turns.</summary>
        public bool IncludePauses { get; set; } = true;

        /// <summary>Duration of pause between turns in ms.</summary>
        public int PauseDurationMs { get; set; } = 500;

        /// <summary>Multiplier for pause durations (1.0 = normal).</summary>
        public double PauseMultiplier { get; set; } = 1.0;

        /// <summary>Whether to normalize audio levels across speakers.</summary>
        public bool NormalizeAudio { get; set; } = true;
    }
}

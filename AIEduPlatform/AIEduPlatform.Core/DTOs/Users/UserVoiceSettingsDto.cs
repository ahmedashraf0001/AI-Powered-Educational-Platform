namespace AIEduPlatform.Core.DTOs.Users
{
    /// <summary>
    /// DTO for reading / writing a user's dialogue voice settings.
    /// </summary>
    public class UserVoiceSettingsDto
    {
        // Voice selection
        public string TeacherVoiceId { get; set; } = "Damien Black";
        public string StudentVoiceId { get; set; } = "Daisy Studious";

        // Speed
        public double TeacherSpeed { get; set; } = 0.95;
        public double StudentSpeed { get; set; } = 1.0;

        // Audio output
        public string OutputFormat { get; set; } = "mp3";
        public int SampleRate { get; set; } = 24000;
        public bool IncludePauses { get; set; } = true;
        public int PauseDurationMs { get; set; } = 500;
        public double PauseMultiplier { get; set; } = 1.0;
        public bool NormalizeAudio { get; set; } = true;
    }
}

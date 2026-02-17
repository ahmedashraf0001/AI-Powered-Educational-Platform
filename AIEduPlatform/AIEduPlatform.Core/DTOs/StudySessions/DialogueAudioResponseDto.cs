namespace AIEduPlatform.Core.DTOs.StudySessions
{
    /// <summary>
    /// Combined response containing the generated dialogue text and its audio representation.
    /// Returned from the GenerateDialogueAudio endpoint.
    /// </summary>
    public class DialogueAudioResponseDto
    {
        /// <summary>
        /// The generated teacher-student dialogue with turns, topic, and sources.
        /// </summary>
        public TeacherStudentDialogue Dialogue { get; set; } = default!;

        /// <summary>
        /// Base64-encoded audio of the dialogue.
        /// </summary>
        public string? AudioBase64 { get; set; }

        /// <summary>
        /// Audio format (e.g., "mp3", "wav").
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// Total audio duration in seconds.
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Audio file size in bytes.
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Total processing time in milliseconds (dialogue generation + audio synthesis).
        /// </summary>
        public double ProcessingTimeMs { get; set; }

        /// <summary>
        /// Per-turn timestamps for syncing text highlights with audio playback.
        /// </summary>
        public IReadOnlyList<TurnTimestamp> TurnTimestamps { get; set; } = [];
    }
}

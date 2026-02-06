using AIEduPlatform.Core.DTOs.Courses;
using System.Text.Json.Serialization;

public interface ITranscriptionService
{
    // ── Speech-to-Text ──────────────────────────────────────

    /// <summary>
    /// POST /transcribe/base64
    /// Takes base64 audio in any language → returns English text.
    /// </summary>
    Task<SpeechToTextResult> TranscribeToEnglishAsync(
        TranscribeAudioRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// POST /transcribe/file
    /// Takes an uploaded audio file in any language → returns English text.
    /// </summary>
    Task<SpeechToTextResult> TranscribeFileAsync(
        Stream audioStream,
        string fileName,
        string fileType, //wav
        string? sourceLanguage = null,
        string task = "translate",
        bool includeTimestamps = true,
        bool includeMetadata = false,
        CancellationToken ct = default);

    /// <summary>
    /// POST /transcribe/batch
    /// Transcribes multiple audio files in one request.
    /// </summary>
    Task<BatchTranscriptionResult> TranscribeBatchAsync(
        BatchTranscriptionRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// GET /transcribe/supported-formats
    /// Returns supported audio formats, max duration, and sample rate.
    /// </summary>
    Task<SupportedFormatsResult> GetSupportedFormatsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// GET /transcribe/supported-languages
    /// Returns all supported languages with dialect info.
    /// </summary>
    Task<SupportedLanguagesResult> GetSupportedInputLanguagesAsync(
        CancellationToken ct = default);

    // ── Text-to-Speech ──────────────────────────────────────

    /// <summary>
    /// GET /synthesize/voices
    /// Returns metadata for all available voices (no audio).
    /// </summary>
    Task<IReadOnlyList<VoiceInfo>> GetAvailableVoicesAsync(
        CancellationToken ct = default);

    /// <summary>
    /// GET /synthesize/voices/preview
    /// Returns all voices with a base64 sample audio clip each.
    /// </summary>
    Task<IReadOnlyList<VoicePreview>> GetVoicePreviewsAsync(
        string? voiceId = null,
        string? sampleText = null,
        string format = "mp3",
        int sampleRate = 48000,
        CancellationToken ct = default);

    /// <summary>
    /// GET /synthesize/voices/default-config
    /// Returns the default voice configuration for dialogues.
    /// </summary>
    Task<DefaultVoiceConfigResult> GetDefaultVoiceConfigAsync(
        CancellationToken ct = default);

    /// <summary>
    /// POST /synthesize/dialogue
    /// Takes an LLM-generated dialogue → returns base64 audio.
    /// </summary>
    Task<DialogueAudioResult> GenerateDialogueAudioAsync(
        TeacherStudentDialogue dialogue,
        DefaultVoiceConfigResult? config = null,
        CancellationToken ct = default);

    /// <summary>
    /// POST /synthesize/synthesize
    /// Takes a single text string → returns base64 audio.
    /// </summary>
    Task<SynthesisResult> SynthesizeTextAsync(
        SynthesizeRequest request,
        CancellationToken ct = default);

}

// ============================================================
// 1. Transcribe Base64 — POST /transcribe/base64
// ============================================================

public sealed record TranscribeAudioRequest(
    string audio,
    string format = "wav",
    string? language = null,
    string task = "translate",
    bool include_timestamps = true,
    bool include_metadata = false);

public sealed record SpeechToTextResult(
    string Text,
    string? Language,
    double? LanguageProbability,
    IReadOnlyList<TranscriptionSegment> Segments,
    string LlmContext,
    double ProcessingTimeMs,
    double AudioDurationSeconds,
    string ModelName);

public sealed record TranscriptionSegment(
    string Text,
    double StartTime,
    double EndTime);

// ============================================================
// 2. Transcribe File — POST /transcribe/file (multipart/form-data)
// ============================================================
// No request DTO — uses Stream + form fields directly.
// Response reuses SpeechToTextResult.

// ============================================================
// 3. Transcribe Batch — POST /transcribe/batch
// ============================================================

public sealed record BatchTranscriptionRequest(
    IReadOnlyList<BatchAudioItem> audio_files,
    string? global_language = null,
    string task = "translate",
    bool include_timestamps = true,
    bool continue_on_error = true);

public sealed record BatchAudioItem(
    int Index,
    string? audio = null,
    string? Path = null,
    string Format = "wav",
    string? Language = null);

public sealed record BatchTranscriptionResult(
    IReadOnlyList<BatchItemResult> Results,
    int total_files,
    int Successful,
    int Failed,
    double total_processing_time_ms);

public sealed record BatchItemResult(
    int Index,
    bool Success,
    string? Text,
    string? Language,
    string? llm_context,
    double? processing_time_ms,
    string? Error);

// ============================================================
// 4. Supported Formats — GET /transcribe/supported-formats
// ============================================================

public sealed record SupportedFormatsResult(
    IReadOnlyList<string> SupportedFormats,
    int MaxDurationSeconds,
    int SampleRate);

// ============================================================
// 5. Supported Languages — GET /transcribe/supported-languages
// ============================================================

public sealed record SupportedLanguagesResult(
    IReadOnlyDictionary<string, string> Languages,
    bool AutoDetect,
    string DefaultTask,
    string OutputLanguage,
    string Note,
    ArabicSupportInfo ArabicSupport);

public sealed record ArabicSupportInfo(
    string Code,
    IReadOnlyList<string> DialectsSupported,
    string DefaultOutput);

// ============================================================
// 6. Voices (metadata only) — GET /synthesize/voices
// ============================================================

public sealed record VoiceInfo(
    string VoiceId,
    string Name,
    string? Description,
    string? Gender,
    IReadOnlyList<string> Languages,
    bool RecommendedForTeacher,
    bool RecommendedForStudent,
    string? PreviewUrl);

// ============================================================
// 7. Voice Previews (with audio) — GET /synthesize/voices/preview
// ============================================================

public sealed record VoicePreview(
    string VoiceId,
    string Name,
    string? Description,
    string? Gender,
    IReadOnlyList<string> Languages,
    bool RecommendedForTeacher,
    bool RecommendedForStudent,
    string SampleText,
    string? AudioBase64,
    string Format,
    double DurationSeconds,
    long FileSizeBytes,
    int SampleRate,
    bool Success,
    string? ErrorMessage);

// ============================================================
// 8. Default Voice Config — GET /synthesize/voices/default-config
// ============================================================

public sealed record DefaultVoiceConfigResult(
    string TeacherVoiceId,
    string StudentVoiceId,
    double TeacherSpeed,
    double StudentSpeed,
    string? TeacherVoiceName,
    string? StudentVoiceName);

// ============================================================
// 9. Generate Dialogue — POST /synthesize/dialogue
// ============================================================

public sealed record DialogueRequest(
    IReadOnlyList<DialogueTurn> Turns,
    string? Topic = null,
    DialogueVoiceConfig? VoiceConfig = null,
    string OutputFormat = "mp3",
    int SampleRate = 48000,
    bool IncludePauses = true,
    int PauseDurationMs = 500,
    double PauseMultiplier = 1.0,
    bool NormalizeAudio = true);

public sealed record DialogueTurn(
    string Speaker,
    string Text);

public sealed record DialogueVoiceConfig(
    string TeacherVoiceId = "p267",
    string StudentVoiceId = "p230",
    double TeacherSpeed = 0.95,
    double StudentSpeed = 1.0);

public sealed record DialogueAudioResult(
    bool Success,
    string? error_message,
    string Format,
    double duration_seconds,
    long file_size_bytes,
    double processing_time_ms,
    IReadOnlyList<TurnTimestamp> turn_timestamps,
    string? audio_base64);


public sealed record TurnTimestamp(
    int TurnIndex,
    string Speaker,
    string Text,
    double StartTime,
    double EndTime,
    double Duration);
public class TeacherStudentDialogue
{
    public string Topic { get; set; } = default!;
    public string Summary { get; set; } = default!;
    public List<TurnDto> Turns { get; set; } = new();
    public List<SourceDto> Sources { get; set; } = new();
    public int EstimatedDurationSeconds { get; set; }
}
public class TurnDto
{
    public string Speaker { get; set; }
    public string TurnType { get; set; }
    public string Content { get; set; } = default!;
    public string Tone { get; set; } = default!;
    public double PauseAfterSeconds { get; set; }
}
public class SourceDto
{
    public string Title { get; set; } = default!;
    public string Location { get; set; } = default!;
    public string ReferencedConcept { get; set; } = default!;
}
// ============================================================
// 10. Synthesize Single Text — POST /synthesize/synthesize
// ============================================================

public sealed record SynthesizeRequest(
    string Text,
    string VoiceId = "p267",
    double Speed = 1.0,
    string OutputFormat = "mp3");

public sealed record SynthesisResult(
    bool Success,
    string? ErrorMessage,
    string Format,
    double DurationSeconds,
    long FileSizeBytes,
    double ProcessingTimeMs,
    string? AudioBase64);

// ============================================================
// Health — GET /health
// ============================================================

public sealed record ServiceHealthStatus(
    string Status,
    bool SttModelLoaded,
    bool TtsModelLoaded,
    string SttModelName,
    string TtsModelName);
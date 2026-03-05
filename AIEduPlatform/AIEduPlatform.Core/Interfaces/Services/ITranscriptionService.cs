using AIEduPlatform.Core.DTOs.Courses;
using System.Text.Json.Serialization;

public interface ITranscriptionService
{
    // ── Speech-to-Text ──────────────────────────────────────

    /// <summary>
    /// POST /transcribe/file (multipart form)
    /// Takes raw audio bytes via multipart form in any language → returns English text.
    /// </summary>
    Task<SpeechToTextResult> TranscribeToBase64EnglishAsync(
        byte[] audio,
        TranscribeAudioRequestConfig config,
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
        int sampleRate = 24000,
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
        DialogueAudioOptions? audioOptions = null,
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
// 1. Transcription Config
// ============================================================

public sealed record TranscribeAudioRequestConfig(
    [property: JsonPropertyName("format")] string Format = "wav",
    [property: JsonPropertyName("language")] string? Language = null,
    [property: JsonPropertyName("task")] string Task = "translate",
    [property: JsonPropertyName("include_timestamps")] bool IncludeTimestamps = true,
    [property: JsonPropertyName("include_metadata")] bool IncludeMetadata = false
);
public sealed record SpeechToTextResult(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("language_probability")] double? LanguageProbability,
    [property: JsonPropertyName("segments")] IReadOnlyList<TranscriptionSegment> Segments,
    [property: JsonPropertyName("llm_context")] string LlmContext,
    [property: JsonPropertyName("processing_time_ms")] double ProcessingTimeMs,
    [property: JsonPropertyName("audio_duration_seconds")] double AudioDurationSeconds,
    [property: JsonPropertyName("model_name")] string ModelName
);

public sealed record TranscriptionSegment(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("start_time")] double StartTime,
    [property: JsonPropertyName("end_time")] double EndTime
);

// ============================================================
// 2. Transcribe File — POST /transcribe/file (multipart/form-data)
// ============================================================
// No request DTO — uses Stream + form fields directly.
// Response reuses SpeechToTextResult.

// ============================================================
// 3. Transcribe Batch — POST /transcribe/batch (multipart/form-data)
// ============================================================

/// <summary>
/// Batch transcription request using multipart form data.
/// Audio files are sent as raw bytes (not base64).
/// </summary>
public sealed record BatchTranscriptionRequest(
    IReadOnlyList<BatchAudioFile> AudioFiles,
    string? GlobalLanguage = null,
    string Task = "translate",
    bool IncludeTimestamps = true,
    bool ContinueOnError = true
);

/// <summary>
/// Represents a single audio file in a batch request.
/// Contains raw byte data instead of base64-encoded string.
/// </summary>
public sealed record BatchAudioFile(
    int Index,
    byte[] AudioData,
    string Format = "wav"
);


public sealed record BatchTranscriptionResult(
    [property: JsonPropertyName("results")] IReadOnlyList<BatchItemResult> Results,
    [property: JsonPropertyName("total_files")] int TotalFiles,
    [property: JsonPropertyName("successful")] int Successful,
    [property: JsonPropertyName("failed")] int Failed,
    [property: JsonPropertyName("total_processing_time_ms")] double TotalProcessingTimeMs
);

public sealed record BatchItemResult(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("language")] string? Language,
    [property: JsonPropertyName("llm_context")] string? LlmContext,
    [property: JsonPropertyName("processing_time_ms")] double? ProcessingTimeMs,
    [property: JsonPropertyName("error")] string? Error
);

// ============================================================
// 4. Supported Formats — GET /transcribe/supported-formats
// ============================================================

public sealed record SupportedFormatsResult(
    [property: JsonPropertyName("supported_formats")] IReadOnlyList<string> SupportedFormats,
    [property: JsonPropertyName("max_duration_seconds")] int MaxDurationSeconds,
    [property: JsonPropertyName("sample_rate")] int SampleRate
);

// ============================================================
// 5. Supported Languages — GET /transcribe/supported-languages
// ============================================================

public sealed record SupportedLanguagesResult(
    [property: JsonPropertyName("languages")] IReadOnlyDictionary<string, string> Languages,
    [property: JsonPropertyName("auto_detect")] bool AutoDetect,
    [property: JsonPropertyName("default_task")] string DefaultTask,
    [property: JsonPropertyName("output_language")] string OutputLanguage,
    [property: JsonPropertyName("note")] string Note,
    [property: JsonPropertyName("arabic_support")] ArabicSupportInfo ArabicSupport
);

public sealed record ArabicSupportInfo(
    [property: JsonPropertyName("code")] string Code,
    [property: JsonPropertyName("dialects_supported")] IReadOnlyList<string> DialectsSupported,
    [property: JsonPropertyName("default_output")] string DefaultOutput
);
// ============================================================
// 6. Voices (metadata only) — GET /synthesize/voices
// ============================================================

public sealed record VoiceInfo(
    [property: JsonPropertyName("voice_id")] string VoiceId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("languages")] IReadOnlyList<string> Languages,
    [property: JsonPropertyName("recommended_for_teacher")] bool RecommendedForTeacher,
    [property: JsonPropertyName("recommended_for_student")] bool RecommendedForStudent,
    [property: JsonPropertyName("preview_url")] string? PreviewUrl
);

// ============================================================
// 7. Voice Previews (with audio) — GET /synthesize/voices/preview
// ============================================================
public sealed record VoicePreview(
    [property: JsonPropertyName("voice_id")] string VoiceId,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("gender")] string? Gender,
    [property: JsonPropertyName("languages")] IReadOnlyList<string> Languages,
    [property: JsonPropertyName("recommended_for_teacher")] bool RecommendedForTeacher,
    [property: JsonPropertyName("recommended_for_student")] bool RecommendedForStudent,
    [property: JsonPropertyName("sample_text")] string SampleText,
    [property: JsonPropertyName("audio_base64")] string? AudioBase64,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("duration_seconds")] double DurationSeconds,
    [property: JsonPropertyName("file_size_bytes")] long FileSizeBytes,
    [property: JsonPropertyName("sample_rate")] int SampleRate,
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error_message")] string? ErrorMessage
);

// ============================================================
// 8. Default Voice Config — GET /synthesize/voices/default-config
// ============================================================

public sealed record DefaultVoiceConfigResult(
    [property: JsonPropertyName("teacher_voice_id")] string TeacherVoiceId,
    [property: JsonPropertyName("student_voice_id")] string StudentVoiceId,
    [property: JsonPropertyName("teacher_speed")] double TeacherSpeed,
    [property: JsonPropertyName("student_speed")] double StudentSpeed,
    [property: JsonPropertyName("teacher_voice_name")] string? TeacherVoiceName,
    [property: JsonPropertyName("student_voice_name")] string? StudentVoiceName
);


// ============================================================
// 9. Generate Dialogue — POST /synthesize/dialogue
// ============================================================


public sealed record DialogueRequest(
    [property: JsonPropertyName("turns")] IReadOnlyList<DialogueTurn> Turns,
    [property: JsonPropertyName("topic")] string? Topic = null,
    [property: JsonPropertyName("voice_config")] DialogueVoiceConfig? VoiceConfig = null,
    [property: JsonPropertyName("output_format")] string OutputFormat = "mp3",
    [property: JsonPropertyName("sample_rate")] int SampleRate = 24000,
    [property: JsonPropertyName("include_pauses")] bool IncludePauses = true,
    [property: JsonPropertyName("pause_duration_ms")] int PauseDurationMs = 500,
    [property: JsonPropertyName("pause_multiplier")] double PauseMultiplier = 1.0,
    [property: JsonPropertyName("normalize_audio")] bool NormalizeAudio = true
);

public sealed record DialogueTurn(
    [property: JsonPropertyName("speaker")] string Speaker,
    [property: JsonPropertyName("text")] string Text
);

public sealed record DialogueVoiceConfig(
    [property: JsonPropertyName("teacher_voice_id")] string TeacherVoiceId = "Damien Black",
    [property: JsonPropertyName("student_voice_id")] string StudentVoiceId = "Daisy Studious",
    [property: JsonPropertyName("teacher_speed")] double TeacherSpeed = 0.95,
    [property: JsonPropertyName("student_speed")] double StudentSpeed = 1.0
);

public sealed record DialogueAudioOptions(
    string OutputFormat = "mp3",
    int SampleRate = 24000,
    bool IncludePauses = true,
    int PauseDurationMs = 500,
    double PauseMultiplier = 1.0,
    bool NormalizeAudio = true
);

public sealed record DialogueAudioResult(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error_message")] string? ErrorMessage,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("duration_seconds")] double DurationSeconds,
    [property: JsonPropertyName("file_size_bytes")] long FileSizeBytes,
    [property: JsonPropertyName("processing_time_ms")] double ProcessingTimeMs,
    [property: JsonPropertyName("turn_timestamps")] IReadOnlyList<TurnTimestamp> TurnTimestamps,
    [property: JsonPropertyName("audio_base64")] string? AudioBase64
);

public sealed record TurnTimestamp(
    [property: JsonPropertyName("turn_index")] int TurnIndex,
    [property: JsonPropertyName("speaker")] string Speaker,
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("start_time")] double StartTime,
    [property: JsonPropertyName("end_time")] double EndTime,
    [property: JsonPropertyName("duration")] double Duration
);
public class TeacherStudentDialogue
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = default!;

    [JsonPropertyName("summary")]
    public string Summary { get; set; } = default!;

    [JsonPropertyName("turns")]
    public List<TurnDto> Turns { get; set; } = new();

    [JsonPropertyName("sources")]
    public List<SourceDto> Sources { get; set; } = new();

    [JsonPropertyName("estimatedDurationSeconds")]
    public int EstimatedDurationSeconds { get; set; }
}

public class TurnDto
{
    [JsonPropertyName("speaker")]
    public string Speaker { get; set; }

    [JsonPropertyName("turnType")]
    public string TurnType { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = default!;

    [JsonPropertyName("tone")]
    public string Tone { get; set; } = default!;

    [JsonPropertyName("pauseAfterSeconds")]
    public double PauseAfterSeconds { get; set; }
}

public class SourceDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    [JsonPropertyName("location")]
    public string Location { get; set; } = default!;

    [JsonPropertyName("referencedConcept")]
    public string ReferencedConcept { get; set; } = default!;
}
// ============================================================
// 10. Synthesize Single Text — POST /synthesize/synthesize
// ============================================================

public sealed record SynthesizeRequest(
    [property: JsonPropertyName("text")] string Text,
    [property: JsonPropertyName("voice_id")] string VoiceId = "Damien Black",
    [property: JsonPropertyName("speed")] double Speed = 1.0,
    [property: JsonPropertyName("output_format")] string OutputFormat = "mp3"
);

public sealed record SynthesisResult(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("error_message")] string? ErrorMessage,
    [property: JsonPropertyName("format")] string Format,
    [property: JsonPropertyName("duration_seconds")] double DurationSeconds,
    [property: JsonPropertyName("file_size_bytes")] long FileSizeBytes,
    [property: JsonPropertyName("processing_time_ms")] double ProcessingTimeMs,
    [property: JsonPropertyName("audio_base64")] string? AudioBase64
);

// ============================================================
// Health — GET /health
// ============================================================

public sealed record ServiceHealthStatus(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("stt_model_loaded")] bool SttModelLoaded,
    [property: JsonPropertyName("tts_model_loaded")] bool TtsModelLoaded,
    [property: JsonPropertyName("stt_model_name")] string SttModelName,
    [property: JsonPropertyName("tts_model_name")] string TtsModelName
);

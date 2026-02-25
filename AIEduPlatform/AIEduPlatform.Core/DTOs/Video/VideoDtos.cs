using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Video
{
    public class VideoAnalysisRequest
    {
        [JsonPropertyName("frame_interval_seconds")]
        public float FrameIntervalSeconds { get; set; } = 5.0f;

        [JsonPropertyName("max_frames")]
        public int MaxFrames { get; set; } = 200;

        [JsonPropertyName("transcribe")]
        public bool Transcribe { get; set; } = true;

        [JsonPropertyName("analyze_visuals")]
        public bool AnalyzeVisuals { get; set; } = true;

        [JsonPropertyName("language")]
        public string Language { get; set; } = "en";

        [JsonPropertyName("include_timestamps")]
        public bool IncludeTimestamps { get; set; } = true;

        [JsonPropertyName("summary_format")]
        public bool SummaryFormat { get; set; } = false;
    }

    public class VideoAnalysisResponse
    {
        [JsonPropertyName("segments")]
        public List<Segment> Segments { get; set; } = new();

        [JsonPropertyName("full_transcript")]
        public string FullTranscript { get; set; } = string.Empty;

        [JsonPropertyName("frame_analyses")]
        public List<FrameAnalyses> FrameAnalyses { get; set; } = new();

        [JsonPropertyName("llm_context")]
        public string LlmContext { get; set; } = string.Empty;

        [JsonPropertyName("video_duration_seconds")]
        public float VideoDurationSeconds { get; set; }

        [JsonPropertyName("processing_time_ms")]
        public float ProcessingTimeMs { get; set; }

        [JsonPropertyName("video_dimensions")]
        public VideoDimensions VideoDimensions { get; set; } = new();

        [JsonPropertyName("fps")]
        public float Fps { get; set; }

        [JsonPropertyName("has_audio")]
        public bool HasAudio { get; set; }

        [JsonPropertyName("frames_analyzed")]
        public int FramesAnalyzed { get; set; }
    }

    public class VideoDimensions
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }
    }

    public class Segment
    {
        [JsonPropertyName("start_time")]
        public float StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public float EndTime { get; set; }

        [JsonPropertyName("visual_description")]
        public string VisualDescription { get; set; } = string.Empty;

        [JsonPropertyName("transcript")]
        public string Transcript { get; set; } = string.Empty;
    }

    public class FrameAnalyses
    {
        [JsonPropertyName("timestamp_seconds")]
        public float TimestampSeconds { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("frame_number")]
        public int FrameNumber { get; set; }
    }
}

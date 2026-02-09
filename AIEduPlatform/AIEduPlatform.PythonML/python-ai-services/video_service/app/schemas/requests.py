from pydantic import BaseModel, Field
from typing import Optional


class VideoAnalysisRequest(BaseModel):
    """Request for video analysis with base64-encoded video."""
    video: str = Field(..., description="Base64-encoded video data")
    frame_interval_seconds: float = Field(5.0, ge=1.0, le=60.0, description="Seconds between frame extractions")
    max_frames: int = Field(50, ge=1, le=200, description="Maximum frames to analyze")
    transcribe: bool = Field(True, description="Whether to transcribe audio")
    analyze_visuals: bool = Field(True, description="Whether to analyze visual frames")
    language: Optional[str] = Field(None, description="Language hint for transcription (e.g., 'en', 'es')")
    include_timestamps: bool = Field(True, description="Include timestamps in LLM context")
    summary_format: bool = Field(False, description="Use compact summary format for LLM context")


class VideoPathRequest(BaseModel):
    """Request for video analysis from file path."""
    path: str = Field(..., description="Path to video file (must be accessible from within the container)")
    frame_interval_seconds: float = Field(5.0, ge=1.0, le=60.0, description="Seconds between frame extractions")
    max_frames: int = Field(50, ge=1, le=200, description="Maximum frames to analyze")
    transcribe: bool = Field(True, description="Whether to transcribe audio")
    analyze_visuals: bool = Field(True, description="Whether to analyze visual frames")
    language: Optional[str] = Field(None, description="Language hint for transcription")
    include_timestamps: bool = Field(True, description="Include timestamps in LLM context")
    summary_format: bool = Field(False, description="Use compact summary format for LLM context")


class SegmentResponse(BaseModel):
    """A unified segment with visual and audio data."""
    start_time: float = Field(..., description="Segment start time in seconds")
    end_time: float = Field(..., description="Segment end time in seconds")
    visual_description: Optional[str] = Field(None, description="Description of visual content")
    transcript: Optional[str] = Field(None, description="Audio transcript for this segment")


class FrameAnalysisResponse(BaseModel):
    """Analysis result for a single frame."""
    timestamp_seconds: float = Field(..., description="Frame timestamp in seconds")
    description: str = Field(..., description="Visual description of the frame")
    frame_number: int = Field(..., description="Frame index number")


class VideoAnalysisResponse(BaseModel):
    """Complete video analysis response."""
    segments: list[SegmentResponse] = Field(..., description="Unified video segments combining visual and audio")
    full_transcript: str = Field(..., description="Complete audio transcript of the video")
    frame_analyses: list[FrameAnalysisResponse] = Field(..., description="Individual frame analyses with timestamps")
    llm_context: str = Field(..., description="Pre-formatted context string for direct LLM consumption")
    video_duration_seconds: float = Field(..., description="Total video duration in seconds")
    processing_time_ms: float = Field(..., description="Total processing time in milliseconds")
    video_dimensions: dict = Field(..., description="Video dimensions (width, height)")
    fps: float = Field(..., description="Video frame rate")
    has_audio: bool = Field(..., description="Whether the video has an audio track")
    frames_analyzed: int = Field(..., description="Number of frames that were analyzed")
    
    class Config:
        json_schema_extra = {
            "example": {
                "segments": [
                    {
                        "start_time": 0.0,
                        "end_time": 5.0,
                        "visual_description": "A professor stands at a whiteboard writing equations",
                        "transcript": "Today we'll discuss the fundamentals of calculus"
                    }
                ],
                "full_transcript": "Today we'll discuss the fundamentals of calculus...",
                "frame_analyses": [
                    {
                        "timestamp_seconds": 0.0,
                        "description": "A professor stands at a whiteboard writing equations",
                        "frame_number": 0
                    }
                ],
                "llm_context": "[Video Analysis - Duration: 05:00]\n\n[00:00 - 00:05]\n  Visual: A professor stands at a whiteboard writing equations\n  Audio: \"Today we'll discuss the fundamentals of calculus\"",
                "video_duration_seconds": 300.0,
                "processing_time_ms": 45000.0,
                "video_dimensions": {"width": 1920, "height": 1080},
                "fps": 30.0,
                "has_audio": True,
                "frames_analyzed": 60
            }
        }

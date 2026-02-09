from fastapi import APIRouter, UploadFile, File, Query, BackgroundTasks
from typing import Optional
from pydantic import BaseModel, Field
import os
import tempfile
import shutil
import aiofiles
import base64

from app.config import get_settings, Settings
from app.middleware.error_handler import VideoProcessingError, ModelError


router = APIRouter()

SUPPORTED_VIDEO_FORMATS = {"mp4", "avi", "mov", "mkv", "webm", "flv", "wmv", "m4v"}


def get_analyzer():
    """Get the global analyzer instance."""
    from app.main import get_analyzer as get_model
    analyzer = get_model()
    if analyzer is None:
        raise ModelError("Video analyzer not initialized")
    return analyzer


def validate_video_format(filename: str) -> None:
    """Validate that the video format is supported."""
    if filename:
        ext = filename.rsplit(".", 1)[-1].lower()
        if ext not in SUPPORTED_VIDEO_FORMATS:
            raise VideoProcessingError(
                f"Unsupported video format: {ext}",
                details={"supported_formats": list(SUPPORTED_VIDEO_FORMATS)}
            )


# Response Models
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
    segments: list[SegmentResponse] = Field(..., description="Unified video segments")
    full_transcript: str = Field(..., description="Complete audio transcript")
    frame_analyses: list[FrameAnalysisResponse] = Field(..., description="Individual frame analyses")
    llm_context: str = Field(..., description="Formatted context for LLM consumption")
    video_duration_seconds: float = Field(..., description="Video duration")
    processing_time_ms: float = Field(..., description="Total processing time")
    video_dimensions: dict = Field(..., description="Video width and height")
    fps: float = Field(..., description="Video frame rate")
    has_audio: bool = Field(..., description="Whether video has audio track")
    frames_analyzed: int = Field(..., description="Number of frames analyzed")


class VideoAnalysisRequest(BaseModel):
    """Request for video analysis with base64-encoded video."""
    video: str = Field(..., description="Base64-encoded video data")
    frame_interval_seconds: float = Field(5.0, description="Seconds between frame extractions")
    max_frames: int = Field(50, description="Maximum frames to analyze")
    transcribe: bool = Field(True, description="Whether to transcribe audio")
    analyze_visuals: bool = Field(True, description="Whether to analyze visual frames")
    language: Optional[str] = Field(None, description="Language hint for transcription")
    include_timestamps: bool = Field(True, description="Include timestamps in LLM context")
    summary_format: bool = Field(False, description="Use compact summary format for LLM context")


class VideoPathRequest(BaseModel):
    """Request for video analysis from file path."""
    path: str = Field(..., description="Path to video file")
    frame_interval_seconds: float = Field(5.0, description="Seconds between frame extractions")
    max_frames: int = Field(50, description="Maximum frames to analyze")
    transcribe: bool = Field(True, description="Whether to transcribe audio")
    analyze_visuals: bool = Field(True, description="Whether to analyze visual frames")
    language: Optional[str] = Field(None, description="Language hint for transcription")
    include_timestamps: bool = Field(True, description="Include timestamps in LLM context")
    summary_format: bool = Field(False, description="Use compact summary format for LLM context")


def cleanup_temp_file(path: str):
    """Remove temporary file."""
    try:
        if os.path.exists(path):
            os.remove(path)
    except Exception:
        pass


@router.post("/analyze", response_model=VideoAnalysisResponse)
async def analyze_video_upload(
    background_tasks: BackgroundTasks,
    video: UploadFile = File(..., description="Video file to analyze"),
    frame_interval_seconds: float = Query(5.0, ge=1.0, le=60.0, description="Seconds between frame extractions"),
    max_frames: int = Query(50, ge=1, le=200, description="Maximum frames to analyze"),
    transcribe: bool = Query(True, description="Whether to transcribe audio"),
    analyze_visuals: bool = Query(True, description="Whether to analyze visual frames"),
    language: Optional[str] = Query(None, description="Language hint for transcription (e.g., 'en', 'es')"),
    include_timestamps: bool = Query(True, description="Include timestamps in LLM context"),
    summary_format: bool = Query(False, description="Use compact summary format")
) -> VideoAnalysisResponse:
    """
    Analyze an uploaded video file.
    
    Extracts visual frames and audio, analyzes them, and returns combined context
    suitable for LLM consumption.
    """
    validate_video_format(video.filename)
    settings = get_settings()
    analyzer = get_analyzer()
    
    # Save uploaded video to temp file
    suffix = os.path.splitext(video.filename)[1] if video.filename else ".mp4"
    temp_file = tempfile.NamedTemporaryFile(delete=False, suffix=suffix, dir=settings.temp_dir)
    temp_path = temp_file.name
    
    try:
        # Write uploaded content to temp file
        async with aiofiles.open(temp_path, 'wb') as f:
            content = await video.read()
            await f.write(content)
        
        # Analyze video
        result = await analyzer.analyze_video_async(
            video_path=temp_path,
            frame_interval_seconds=frame_interval_seconds,
            max_frames=max_frames,
            transcribe=transcribe,
            analyze_visuals=analyze_visuals,
            language=language
        )
        
        # Generate LLM context
        llm_context = result.to_llm_context(
            include_timestamps=include_timestamps,
            summary_format=summary_format
        )
        
        return VideoAnalysisResponse(
            segments=[SegmentResponse(**seg.to_dict()) for seg in result.segments],
            full_transcript=result.full_transcript,
            frame_analyses=[FrameAnalysisResponse(**fa.to_dict()) for fa in result.frame_analyses],
            llm_context=llm_context,
            video_duration_seconds=result.video_duration_seconds,
            processing_time_ms=result.processing_time_ms,
            video_dimensions={"width": result.video_dimensions[0], "height": result.video_dimensions[1]},
            fps=result.fps,
            has_audio=result.has_audio,
            frames_analyzed=result.frames_analyzed
        )
    finally:
        # Schedule cleanup
        background_tasks.add_task(cleanup_temp_file, temp_path)


@router.post("/analyze/base64", response_model=VideoAnalysisResponse)
async def analyze_video_base64(
    request: VideoAnalysisRequest,
    background_tasks: BackgroundTasks
) -> VideoAnalysisResponse:
    """
    Analyze a base64-encoded video.
    
    Useful for programmatic API access without multipart uploads.
    """
    settings = get_settings()
    analyzer = get_analyzer()
    
    # Decode base64 video
    try:
        video_bytes = base64.b64decode(request.video)
    except Exception as e:
        raise VideoProcessingError(f"Invalid base64 video data: {e}")
    
    # Save to temp file
    temp_file = tempfile.NamedTemporaryFile(delete=False, suffix=".mp4", dir=settings.temp_dir)
    temp_path = temp_file.name
    
    try:
        with open(temp_path, 'wb') as f:
            f.write(video_bytes)
        
        # Analyze video
        result = await analyzer.analyze_video_async(
            video_path=temp_path,
            frame_interval_seconds=request.frame_interval_seconds,
            max_frames=request.max_frames,
            transcribe=request.transcribe,
            analyze_visuals=request.analyze_visuals,
            language=request.language
        )
        
        llm_context = result.to_llm_context(
            include_timestamps=request.include_timestamps,
            summary_format=request.summary_format
        )
        
        return VideoAnalysisResponse(
            segments=[SegmentResponse(**seg.to_dict()) for seg in result.segments],
            full_transcript=result.full_transcript,
            frame_analyses=[FrameAnalysisResponse(**fa.to_dict()) for fa in result.frame_analyses],
            llm_context=llm_context,
            video_duration_seconds=result.video_duration_seconds,
            processing_time_ms=result.processing_time_ms,
            video_dimensions={"width": result.video_dimensions[0], "height": result.video_dimensions[1]},
            fps=result.fps,
            has_audio=result.has_audio,
            frames_analyzed=result.frames_analyzed
        )
    finally:
        background_tasks.add_task(cleanup_temp_file, temp_path)


@router.post("/analyze/path", response_model=VideoAnalysisResponse)
async def analyze_video_path(request: VideoPathRequest) -> VideoAnalysisResponse:
    """
    Analyze a video from a local file path.
    
    The path must be accessible from within the container (use mounted volumes).
    """
    analyzer = get_analyzer()
    
    if not os.path.exists(request.path):
        raise VideoProcessingError(f"Video file not found: {request.path}")
    
    # Validate format
    validate_video_format(request.path)
    
    result = await analyzer.analyze_video_async(
        video_path=request.path,
        frame_interval_seconds=request.frame_interval_seconds,
        max_frames=request.max_frames,
        transcribe=request.transcribe,
        analyze_visuals=request.analyze_visuals,
        language=request.language
    )
    
    llm_context = result.to_llm_context(
        include_timestamps=request.include_timestamps,
        summary_format=request.summary_format
    )
    
    return VideoAnalysisResponse(
        segments=[SegmentResponse(**seg.to_dict()) for seg in result.segments],
        full_transcript=result.full_transcript,
        frame_analyses=[FrameAnalysisResponse(**fa.to_dict()) for fa in result.frame_analyses],
        llm_context=llm_context,
        video_duration_seconds=result.video_duration_seconds,
        processing_time_ms=result.processing_time_ms,
        video_dimensions={"width": result.video_dimensions[0], "height": result.video_dimensions[1]},
        fps=result.fps,
        has_audio=result.has_audio,
        frames_analyzed=result.frames_analyzed
    )


@router.get("/context/{path:path}")
async def get_video_context(
    path: str,
    frame_interval: float = Query(5.0, description="Seconds between frames"),
    max_frames: int = Query(30, description="Max frames to analyze"),
    include_timestamps: bool = Query(True, description="Include timestamps"),
    summary_format: bool = Query(False, description="Use summary format")
) -> dict:
    """
    Quick endpoint to get just the LLM context from a video path.
    
    Returns only the formatted context string, optimized for immediate LLM use.
    """
    analyzer = get_analyzer()
    
    if not os.path.exists(path):
        raise VideoProcessingError(f"Video file not found: {path}")
    
    result = await analyzer.analyze_video_async(
        video_path=path,
        frame_interval_seconds=frame_interval,
        max_frames=max_frames
    )
    
    return {
        "context": result.to_llm_context(
            include_timestamps=include_timestamps,
            summary_format=summary_format
        ),
        "duration_seconds": result.video_duration_seconds,
        "segments_count": len(result.segments)
    }

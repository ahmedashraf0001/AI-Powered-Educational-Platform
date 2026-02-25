from pydantic import BaseModel, Field
from typing import Optional, List


class TranscriptionRequest(BaseModel):
    """Base transcription request model."""
    language: Optional[str] = Field(
        None, 
        description="Language code (e.g., 'en', 'es'). Auto-detect if not provided"
    )
    task: str = Field(
        "transcribe", 
        description="'transcribe' for same-language or 'translate' for translation to English"
    )
    include_timestamps: bool = Field(
        True, 
        description="Whether to include word/segment timestamps in the response"
    )


class TranscriptionSegment(BaseModel):
    """A segment of transcribed audio with timestamps."""
    text: str = Field(..., description="Transcribed text for this segment")
    start_time: float = Field(..., description="Start time in seconds")
    end_time: float = Field(..., description="End time in seconds")


class TranscriptionResult(BaseModel):
    """Complete transcription result."""
    model_config = {"protected_namespaces": ()}
    
    text: str = Field(..., description="Full transcribed text")
    language: Optional[str] = Field(None, description="Detected or specified language")
    language_probability: Optional[float] = Field(
        None, 
        description="Confidence of language detection (0-1)"
    )
    segments: List[TranscriptionSegment] = Field(
        default_factory=list, 
        description="List of timestamped segments"
    )
    processing_time_ms: float = Field(..., description="Time taken to process in milliseconds")
    audio_duration_seconds: float = Field(..., description="Duration of the audio in seconds")
    model_name: str = Field(..., description="Name of the model used")
    
    def to_llm_context(self, include_timestamps: bool = False) -> str:
        """Format transcription as context for LLM consumption."""
        if include_timestamps and self.segments:
            lines = []
            for seg in self.segments:
                timestamp = f"[{seg.start_time:.2f}s - {seg.end_time:.2f}s]"
                lines.append(f"{timestamp} {seg.text}")
            return "\n".join(lines)
        return self.text

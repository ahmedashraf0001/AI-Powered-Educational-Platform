from fastapi import APIRouter, Depends, Query
from typing import Optional, List
from pydantic import BaseModel, Field
import asyncio

from app.config import get_settings, Settings
from app.middleware.error_handler import ModelError, AudioProcessingError


router = APIRouter()


def get_synthesizer():
    """Get the global synthesizer instance."""
    from app.main import get_synthesizer as get_model
    synthesizer = get_model()
    if synthesizer is None:
        raise ModelError("Audio synthesizer not initialized")
    return synthesizer


# ============== Request/Response Models ==============

class VoiceInfoResponse(BaseModel):
    """Voice information response."""
    voice_id: str = Field(..., description="Unique voice identifier")
    name: str = Field(..., description="Display name")
    description: Optional[str] = Field(None, description="Voice description")
    gender: Optional[str] = Field(None, description="Voice gender: male, female, neutral")
    languages: List[str] = Field(default_factory=list, description="Supported languages")
    recommended_for_teacher: bool = Field(False, description="Recommended for teacher role")
    recommended_for_student: bool = Field(False, description="Recommended for student role")
    preview_url: Optional[str] = Field(None, description="Preview audio URL")


class DialogueVoiceConfigRequest(BaseModel):
    """Voice configuration for dialogue generation."""
    teacher_voice_id: str = Field("p267", description="Voice ID for teacher")
    student_voice_id: str = Field("p230", description="Voice ID for student")
    teacher_speed: float = Field(0.95, ge=0.5, le=2.0, description="Teacher speech speed")
    student_speed: float = Field(1.0, ge=0.5, le=2.0, description="Student speech speed")


class DialogueTurnRequest(BaseModel):
    """A single dialogue turn."""
    speaker: str = Field(..., description="Speaker: 'teacher' or 'student'")
    text: str = Field(..., description="Text content for this turn")


class GenerateDialogueRequest(BaseModel):
    """Request for dialogue audio generation."""
    turns: List[DialogueTurnRequest] = Field(..., description="List of dialogue turns")
    topic: Optional[str] = Field(None, description="Topic of the dialogue")
    voice_config: Optional[DialogueVoiceConfigRequest] = Field(None, description="Voice configuration")
    output_format: str = Field("mp3", description="Output format: mp3, wav, ogg")
    sample_rate: int = Field(22050, description="Audio sample rate in Hz")
    include_pauses: bool = Field(True, description="Include pauses between turns")
    pause_duration_ms: int = Field(500, ge=0, le=3000, description="Pause duration in ms")
    pause_multiplier: float = Field(1.0, ge=0.1, le=3.0, description="Pause duration multiplier")
    normalize_audio: bool = Field(True, description="Normalize audio levels")


class TurnTimestampResponse(BaseModel):
    """Timestamp for a dialogue turn."""
    turn_index: int = Field(..., description="Turn index")
    speaker: str = Field(..., description="Speaker of this turn")
    text: str = Field(..., description="Text content spoken in this turn")
    start_time: float = Field(..., description="Start time in seconds")
    end_time: float = Field(..., description="End time in seconds")
    duration: float = Field(..., description="Duration in seconds")


class DialogueAudioResponse(BaseModel):
    """Response for dialogue audio generation."""
    success: bool = Field(..., description="Whether generation succeeded")
    error_message: Optional[str] = Field(None, description="Error message if failed")
    format: str = Field(..., description="Audio format")
    duration_seconds: float = Field(..., description="Total duration")
    file_size_bytes: int = Field(..., description="File size in bytes")
    processing_time_ms: float = Field(..., description="Processing time")
    turn_timestamps: List[TurnTimestampResponse] = Field(default_factory=list, description="Turn timestamps")
    audio_base64: Optional[str] = Field(None, description="Base64 encoded audio data")


class SynthesizeTextRequest(BaseModel):
    """Request for single text synthesis."""
    text: str = Field(..., description="Text to synthesize")
    voice_id: str = Field("p267", description="Voice ID to use")
    speed: float = Field(1.0, ge=0.5, le=2.0, description="Speech speed")
    output_format: str = Field("mp3", description="Output format: mp3, wav, ogg")


class SynthesizeTextResponse(BaseModel):
    """Response for text synthesis."""
    success: bool = Field(..., description="Whether synthesis succeeded")
    error_message: Optional[str] = Field(None, description="Error message if failed")
    format: str = Field(..., description="Audio format")
    duration_seconds: float = Field(..., description="Audio duration")
    file_size_bytes: int = Field(..., description="File size")
    processing_time_ms: float = Field(..., description="Processing time")
    audio_base64: Optional[str] = Field(None, description="Base64 encoded audio")


class DefaultVoiceConfigResponse(BaseModel):
    """Default voice configuration response."""
    teacher_voice_id: str
    student_voice_id: str
    teacher_speed: float
    student_speed: float
    teacher_voice_name: Optional[str]
    student_voice_name: Optional[str]


class VoicePreviewResponse(BaseModel):
    """Voice preview with sample audio."""
    voice_id: str = Field(..., description="Unique voice identifier")
    name: str = Field(..., description="Display name")
    description: Optional[str] = Field(None, description="Voice description")
    gender: Optional[str] = Field(None, description="Voice gender")
    languages: List[str] = Field(default_factory=list, description="Supported languages")
    recommended_for_teacher: bool = Field(False, description="Recommended for teacher role")
    recommended_for_student: bool = Field(False, description="Recommended for student role")
    sample_text: str = Field(..., description="Text that was spoken in the preview")
    audio_base64: Optional[str] = Field(None, description="Base64-encoded sample audio clip")
    format: str = Field("mp3", description="Audio format of the sample")
    duration_seconds: float = Field(0, description="Duration of the sample clip")
    file_size_bytes: int = Field(0, description="Size of the audio in bytes")
    sample_rate: int = Field(22050, description="Audio sample rate in Hz")
    success: bool = Field(True, description="Whether preview generation succeeded")
    error_message: Optional[str] = Field(None, description="Error if generation failed")


# ============== Endpoints ==============

@router.get("/voices", response_model=List[VoiceInfoResponse])
async def get_available_voices() -> List[VoiceInfoResponse]:
    """
    Get list of available voices for text-to-speech.
    
    Returns voices suitable for teacher and student roles.
    """
    synthesizer = get_synthesizer()
    voices = await asyncio.to_thread(synthesizer.get_available_voices)
    
    return [
        VoiceInfoResponse(
            voice_id=v.voice_id,
            name=v.name,
            description=v.description,
            gender=v.gender,
            languages=v.languages,
            recommended_for_teacher=v.recommended_for_teacher,
            recommended_for_student=v.recommended_for_student,
            preview_url=v.preview_url
        )
        for v in voices
    ]


@router.get("/voices/preview", response_model=List[VoicePreviewResponse])
async def get_voice_previews(
    voice_id: Optional[str] = Query(None, description="Specific voice ID to preview. Omit for all voices."),
    sample_text: Optional[str] = Query(None, description="Custom text to speak. Uses a role-appropriate default if omitted."),
    format: str = Query("mp3", description="Audio format: mp3, wav, ogg"),
    sample_rate: int = Query(22050, description="Audio sample rate in Hz")
) -> List[VoicePreviewResponse]:
    """
    Generate audio preview samples so you can hear each voice before choosing.

    Pass no voice_id to get previews of ALL voices.
    Pass a specific voice_id to preview just that one.
    Optionally provide custom sample_text.
    """
    synthesizer = get_synthesizer()

    previews = await asyncio.to_thread(
        synthesizer.generate_voice_preview,
        voice_id=voice_id,
        sample_text=sample_text,
        output_format=format,
        sample_rate=sample_rate
    )

    return [
        VoicePreviewResponse(
            voice_id=p.voice_id,
            name=p.name,
            description=p.description,
            gender=p.gender,
            languages=p.languages,
            recommended_for_teacher=p.recommended_for_teacher,
            recommended_for_student=p.recommended_for_student,
            sample_text=p.sample_text,
            audio_base64=p.audio_base64,
            format=p.format,
            duration_seconds=p.duration_seconds,
            file_size_bytes=p.file_size_bytes,
            sample_rate=p.sample_rate,
            success=p.success,
            error_message=p.error_message
        )
        for p in previews
    ]


@router.get("/voices/default-config", response_model=DefaultVoiceConfigResponse)
async def get_default_voice_config() -> DefaultVoiceConfigResponse:
    """
    Get the default voice configuration for teacher-student dialogues.
    """
    synthesizer = get_synthesizer()
    config = await asyncio.to_thread(synthesizer.get_default_voice_configuration)
    
    return DefaultVoiceConfigResponse(
        teacher_voice_id=config.teacher_voice_id,
        student_voice_id=config.student_voice_id,
        teacher_speed=config.teacher_speed,
        student_speed=config.student_speed,
        teacher_voice_name=config.teacher_voice_name,
        student_voice_name=config.student_voice_name
    )


@router.post("/dialogue", response_model=DialogueAudioResponse)
async def generate_dialogue_audio(
    request: GenerateDialogueRequest,
    settings: Settings = Depends(get_settings)
) -> DialogueAudioResponse:
    """
    Generate audio from a teacher-student dialogue.
    
    Uses different voices for teacher and student speakers.
    Returns audio with timestamps for each dialogue turn.
    """
    import base64
    from app.models.synthesizer import DialogueTurn, TeacherStudentDialogue, DialogueVoiceConfiguration
    
    synthesizer = get_synthesizer()
    
    # Convert request to model objects
    turns = [
        DialogueTurn(speaker=t.speaker, text=t.text)
        for t in request.turns
    ]
    dialogue = TeacherStudentDialogue(turns=turns, topic=request.topic)
    
    # Voice configuration
    voice_config = None
    if request.voice_config:
        voice_config = DialogueVoiceConfiguration(
            teacher_voice_id=request.voice_config.teacher_voice_id,
            student_voice_id=request.voice_config.student_voice_id,
            teacher_speed=request.voice_config.teacher_speed,
            student_speed=request.voice_config.student_speed
        )
    
    # Generate audio (offload to thread to avoid blocking event loop)
    result = await asyncio.to_thread(
        synthesizer.generate_dialogue_audio,
        dialogue=dialogue,
        voice_config=voice_config,
        output_format=request.output_format,
        sample_rate=request.sample_rate,
        include_pauses=request.include_pauses,
        pause_duration_ms=request.pause_duration_ms,
        pause_multiplier=request.pause_multiplier,
        normalize_audio=request.normalize_audio
    )
    
    # Encode audio to base64 if successful
    audio_base64 = None
    if result.success and result.audio_data:
        audio_base64 = base64.b64encode(result.audio_data).decode('utf-8')
    
    return DialogueAudioResponse(
        success=result.success,
        error_message=result.error_message,
        format=result.format,
        duration_seconds=result.duration_seconds,
        file_size_bytes=result.file_size_bytes,
        processing_time_ms=result.processing_time_ms,
        turn_timestamps=[
            TurnTimestampResponse(
                turn_index=t.turn_index,
                speaker=t.speaker,
                text=t.text,
                start_time=t.start_time,
                end_time=t.end_time,
                duration=t.duration
            )
            for t in result.turn_timestamps
        ],
        audio_base64=audio_base64
    )


@router.post("/synthesize", response_model=SynthesizeTextResponse)
async def synthesize_text(
    request: SynthesizeTextRequest,
    settings: Settings = Depends(get_settings)
) -> SynthesizeTextResponse:
    """
    Synthesize speech from a single text.
    
    Useful for generating individual audio clips.
    """
    import base64
    
    synthesizer = get_synthesizer()
    
    result = await asyncio.to_thread(
        synthesizer.synthesize_single,
        text=request.text,
        voice_id=request.voice_id,
        speed=request.speed,
        output_format=request.output_format
    )
    
    audio_base64 = None
    if result.get("success") and result.get("audio_data"):
        audio_base64 = base64.b64encode(result["audio_data"]).decode('utf-8')
    
    return SynthesizeTextResponse(
        success=result.get("success", False),
        error_message=result.get("error_message"),
        format=request.output_format,
        duration_seconds=result.get("duration_seconds", 0),
        file_size_bytes=result.get("file_size_bytes", 0),
        processing_time_ms=result.get("processing_time_ms", 0),
        audio_base64=audio_base64
    )

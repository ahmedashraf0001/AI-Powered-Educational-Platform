from fastapi import APIRouter, Depends
from pydantic import BaseModel
from typing import Dict, Any

from app.config import get_settings, Settings


router = APIRouter()


class HealthResponse(BaseModel):
    """Health check response model."""
    status: str
    service: str
    version: str
    transcription_model: str
    tts_model: str
    gpu_enabled: bool


class ReadinessResponse(BaseModel):
    """Readiness check response model."""
    ready: bool
    details: Dict[str, Any]


@router.get("", response_model=HealthResponse)
@router.get("/", response_model=HealthResponse)
async def health_check(settings: Settings = Depends(get_settings)) -> HealthResponse:
    """Basic health check endpoint."""
    return HealthResponse(
        status="healthy",
        service=settings.app_name,
        version=settings.app_version,
        transcription_model=settings.whisper_model_size,
        tts_model=settings.tts_model_name,
        gpu_enabled=settings.use_gpu and settings.tts_use_gpu
    )


@router.get("/ready", response_model=ReadinessResponse)
async def readiness_check(settings: Settings = Depends(get_settings)) -> ReadinessResponse:
    """Readiness check - verifies all models are loaded and ready."""
    from app.main import get_transcriber, get_synthesizer
    
    transcriber = get_transcriber()
    synthesizer = get_synthesizer()
    
    transcriber_ready = transcriber is not None
    synthesizer_ready = synthesizer is not None
    is_ready = transcriber_ready and synthesizer_ready
    
    details = {
        "speech_to_text": {
            "ready": transcriber_ready,
            "model_size": settings.whisper_model_size,
            "supported_formats": settings.supported_formats,
            "output_language": "en"
        },
        "text_to_speech": {
            "ready": synthesizer_ready,
            "model": settings.tts_model_name,
            "available_voices": 12
        }
    }
    
    return ReadinessResponse(ready=is_ready, details=details)

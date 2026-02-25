from pydantic_settings import BaseSettings, SettingsConfigDict
from typing import List, Optional
from functools import lru_cache


class Settings(BaseSettings):
    """Application settings for Transcription service."""
    
    model_config = SettingsConfigDict(
        env_prefix="TRANSCRIPTION_",
        env_file=".env",
        protected_namespaces=()  # Allow field names starting with 'model_'
    )
    
    # Service configuration
    app_name: str = "Audio Transcription & Synthesis Service"
    app_version: str = "1.0.0"
    debug: bool = False
    
    # ============== Speech-to-Text (Whisper) Settings ==============
    # Model configuration
    whisper_model_name: str = "openai/whisper-small"
    whisper_model_size: str = "small"  # tiny, base, small, medium, large
    use_gpu: bool = True
    
    # Transcription settings
    language: Optional[str] = None  # Auto-detect if None, supports 99+ languages including Arabic
    task: str = "translate"  # "translate" outputs English, "transcribe" keeps original language
    output_language: str = "en"  # All output will be in English
    
    # Audio input settings
    max_audio_duration_seconds: int = 3600  # 1 hour max
    supported_formats: List[str] = ["mp3", "wav", "flac", "ogg", "m4a", "webm", "mp4"]
    sample_rate: int = 48000
    
    # Processing settings
    chunk_length_seconds: int = 30
    batch_size: int = 8
    
    # ============== Text-to-Speech (TTS) Settings ==============
    tts_model_name: str = "tts_models/en/vctk/vits"  # Multi-speaker TTS model
    tts_use_gpu: bool = True
    
    # Default voice settings
    default_teacher_voice: str = "p286"  # Male British voice
    default_student_voice: str = "p270"  # Female British voice
    default_teacher_speed: float = 1.0
    default_student_speed: float = 1.0
    
    # TTS output settings
    tts_sample_rate: int = 48000
    default_output_format: str = "mp3"
    default_pause_duration_ms: int = 1000


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()

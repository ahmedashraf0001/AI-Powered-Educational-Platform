from pydantic_settings import BaseSettings
from typing import Optional
from functools import lru_cache


class Settings(BaseSettings):
    """Application settings for Video Analysis service."""
    
    # Service configuration
    app_name: str = "Video Analysis Service"
    app_version: str = "1.0.0"
    debug: bool = False
    
    # Upstream service URLs (Docker internal networking)
    vision_service_url: str = "http://vision-service:8004"
    transcription_service_url: str = "http://transcription-service:8005"
    
    # Video processing settings
    max_video_duration_seconds: int = 1800  # 30 minutes max
    frame_extraction_interval_seconds: float = 5.0
    max_frames: int = 100
    temp_dir: str = "/tmp/video_processing"
    
    # HTTP client settings
    request_timeout_seconds: int = 120
    
    class Config:
        env_prefix = "VIDEO_"
        env_file = ".env"


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()

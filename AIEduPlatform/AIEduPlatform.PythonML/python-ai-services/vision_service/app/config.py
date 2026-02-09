from pydantic_settings import BaseSettings, SettingsConfigDict
from typing import List
from functools import lru_cache


class Settings(BaseSettings):
    """Application settings for Vision service."""
    
    model_config = SettingsConfigDict(
        env_prefix="VISION_",
        env_file=".env",
        protected_namespaces=()
    )
    
    # Service configuration
    app_name: str = "Vision Analysis Service"
    app_version: str = "1.0.0"
    debug: bool = False
    
    # Model configuration
    model_name: str = "Salesforce/blip-image-captioning-large"
    use_gpu: bool = True
    
    # Generation settings
    max_new_tokens: int = 200
    min_new_tokens: int = 20
    num_beams: int = 4
    
    # Image preprocessing
    max_image_size: int = 1024


@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()

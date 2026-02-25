from pydantic_settings import BaseSettings, SettingsConfigDict
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
    
    # Model configuration - Qwen2-VL-2B-Instruct
    model_name: str = "Qwen/Qwen2-VL-2B-Instruct"
    use_gpu: bool = True
    
    # Generation settings
    max_new_tokens: int = 100
    min_new_tokens: int = 10
    num_beams: int = 5  # Not used by Qwen2-VL but kept for API compatibility
    
    # Image preprocessing
    max_image_size: int = 1024

@lru_cache()
def get_settings() -> Settings:
    """Get cached settings instance."""
    return Settings()
from pydantic_settings import BaseSettings
from functools import lru_cache
import torch


def _default_device() -> str:
    return "cuda" if torch.cuda.is_available() else "cpu"


class Settings(BaseSettings):
    # Application
    APP_NAME: str = "Reranking Service"
    APP_VERSION: str = "1.0.0"
    DEBUG: bool = False
    
    # Model Configuration
    MODEL_NAME: str = "BAAI/bge-reranker-base"
    MODEL_CACHE_DIR: str = "./models"
    
    # Performance
    MAX_BATCH_SIZE: int = 32
    MAX_PAIRS: int = 100
    MAX_TEXT_LENGTH: int = 4096
    DEVICE: str = _default_device()
    
    # API
    CORS_ORIGINS: list = ["*"]
    
    class Config:
        env_file = ".env"

@lru_cache()
def get_settings():
    return Settings()
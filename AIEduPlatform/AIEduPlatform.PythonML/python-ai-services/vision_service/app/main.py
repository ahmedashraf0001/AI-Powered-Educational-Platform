import warnings
import sys
import logging

warnings.filterwarnings("ignore", message=".*resume_download.*", category=FutureWarning)
warnings.filterwarnings("ignore", message=".*TRANSFORMERS_CACHE.*", category=FutureWarning)

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager

from app.config import get_settings
from app.routes import health, vision
from app.middleware.error_handler import add_error_handlers
from app.models.analyzer import VisionAnalyzer

logging.basicConfig(level=logging.INFO, stream=sys.stdout)
logger = logging.getLogger(__name__)

settings = get_settings()

# Global analyzer instance
analyzer: VisionAnalyzer = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager for startup and shutdown."""
    global analyzer
    
    # Startup: Initialize vision analyzer
    logger.info(f"[1/1] Loading vision model: {settings.model_name}")
    sys.stdout.flush()
    analyzer = VisionAnalyzer(
        model_name=settings.model_name,
        use_gpu=settings.use_gpu
    )
    logger.info("Vision model loaded successfully!")
    sys.stdout.flush()
    
    yield
    
    # Shutdown: Cleanup
    logger.info("Shutting down vision service")
    analyzer = None


app = FastAPI(
    title=settings.app_name,
    version=settings.app_version,
    description="Vision analysis service for generating detailed image descriptions to use as LLM context",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Add error handlers
add_error_handlers(app)

# Include routers
app.include_router(health.router, prefix="/health", tags=["Health"])
app.include_router(vision.router, prefix="/vision", tags=["Vision Analysis"])


def get_analyzer() -> VisionAnalyzer:
    """Get the global analyzer instance."""
    return analyzer

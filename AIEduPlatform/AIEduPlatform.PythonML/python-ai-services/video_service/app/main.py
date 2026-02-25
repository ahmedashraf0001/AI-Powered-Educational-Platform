from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager

from app.config import get_settings
from app.routes import health, video
from app.middleware.error_handler import add_error_handlers
from app.models.video_analyzer import VideoAnalyzer


settings = get_settings()

# Global analyzer instance
analyzer: VideoAnalyzer = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager for startup and shutdown."""
    global analyzer
    
    print(f"Initializing video analyzer (orchestrator mode)...", flush=True)
    print(f"  Vision service: {settings.vision_service_url}", flush=True)
    print(f"  Transcription service: {settings.transcription_service_url}", flush=True)
    
    analyzer = VideoAnalyzer(
        vision_service_url=settings.vision_service_url,
        transcription_service_url=settings.transcription_service_url,
        request_timeout_seconds=settings.request_timeout_seconds,
        temp_dir=settings.temp_dir
    )
    print("Video analyzer initialized successfully", flush=True)
    
    yield
    
    # Shutdown: cleanup HTTP client
    print("Shutting down video service", flush=True)
    if analyzer:
        await analyzer.close()
    analyzer = None


app = FastAPI(
    title=settings.app_name,
    version=settings.app_version,
    description="Video analysis orchestrator that extracts frames and audio from videos, "
                "delegates visual analysis to the vision-service and audio transcription "
                "to the transcription-service, then combines results into rich, timestamped "
                "context suitable for LLM consumption.",
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
app.include_router(video.router, prefix="/video", tags=["Video Analysis"])


def get_analyzer() -> VideoAnalyzer:
    """Get the global analyzer instance."""
    return analyzer

import warnings
# Suppress deprecation warnings from external libraries
warnings.filterwarnings("ignore", message=".*resume_download.*", category=FutureWarning)
warnings.filterwarnings("ignore", message=".*TRANSFORMERS_CACHE.*", category=FutureWarning)

import sys
import logging

# Force logging output to be flushed immediately
logging.basicConfig(level=logging.INFO, stream=sys.stdout, force=True)
logger = logging.getLogger(__name__)

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from contextlib import asynccontextmanager

from app.config import get_settings
from app.routes import health, transcription, synthesis
from app.middleware.error_handler import add_error_handlers
from app.models.transcriber import AudioTranscriber
from app.models.synthesizer import AudioSynthesizer


settings = get_settings()

# Global model instances
transcriber: AudioTranscriber = None
synthesizer: AudioSynthesizer = None


@asynccontextmanager
async def lifespan(app: FastAPI):
    """Application lifespan manager for startup and shutdown."""
    global transcriber, synthesizer
    
    # Startup: Initialize audio transcriber (Speech-to-Text)
    logger.info(f"[1/2] Loading Whisper model (size: {settings.whisper_model_size})... This may take a few minutes on first run.")
    sys.stdout.flush()
    transcriber = AudioTranscriber(
        model_size=settings.whisper_model_size,
        use_gpu=settings.use_gpu,
        language=settings.language
    )
    logger.info("[1/2] Whisper transcription model loaded successfully.")
    sys.stdout.flush()
    
    # Startup: Initialize audio synthesizer (Text-to-Speech)
    logger.info(f"[2/2] Loading TTS model ({settings.tts_model_name})... This may take a few minutes on first run.")
    sys.stdout.flush()
    synthesizer = AudioSynthesizer(
        model_name=settings.tts_model_name,
        use_gpu=settings.tts_use_gpu
    )
    logger.info("[2/2] TTS synthesis model loaded successfully.")
    sys.stdout.flush()
    
    logger.info("All models loaded. Service is ready!")
    
    yield
    
    # Shutdown: Cleanup
    print("Shutting down transcription & synthesis service")
    transcriber = None
    synthesizer = None


app = FastAPI(
    title=settings.app_name,
    version=settings.app_version,
    description="""
Audio Transcription & Synthesis Service

**Speech-to-Text (Transcription):**
- Transcribe audio files to English text using Whisper
- Supports 99+ languages including Arabic (Egyptian dialect)
- All output is translated to English for LLM consumption

**Text-to-Speech (Synthesis):**
- Generate audio from teacher-student dialogues
- Multiple voices for realistic conversations
- Configurable speech speed and pauses
""",
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
app.include_router(transcription.router, prefix="/transcribe", tags=["Speech-to-Text"])
app.include_router(synthesis.router, prefix="/synthesize", tags=["Text-to-Speech"])


def get_transcriber() -> AudioTranscriber:
    """Get the global transcriber instance."""
    return transcriber


def get_synthesizer() -> AudioSynthesizer:
    """Get the global synthesizer instance."""
    return synthesizer

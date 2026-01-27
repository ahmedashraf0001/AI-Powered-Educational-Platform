from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.exceptions import RequestValidationError
from contextlib import asynccontextmanager
import logging
import os

from app.config import get_settings
from app.routes import embeddings, health
from app.models.embedder import embedding_model
from app.middleware.error_handler import (
    validation_exception_handler,
    general_exception_handler
)
from app.schemas.requests import HealthResponse

# Suppress HuggingFace warnings
os.environ["HF_HUB_DISABLE_SYMLINKS_WARNING"] = "1"
os.environ["TOKENIZERS_PARALLELISM"] = "false"

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

settings = get_settings()

@asynccontextmanager
async def lifespan(app: FastAPI):
    """Startup and shutdown events"""
    # Startup
    logger.info("Starting Embedding Service...")
    logger.info(f"Loading model: {settings.MODEL_NAME}")
    
    # Trigger model loading
    _ = embedding_model.dimension
    
    logger.info(f"Model loaded. Embedding dimension: {embedding_model.dimension}")
    logger.info("Embedding Service ready!")
    
    yield
    
    # Shutdown
    logger.info("Shutting down Embedding Service...")

# Create FastAPI app
app = FastAPI(
    title=settings.APP_NAME,
    version=settings.APP_VERSION,
    description="High-performance embedding generation service using sentence-transformers",
    lifespan=lifespan
)

# Add CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.CORS_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(
    health.router,
    tags=["health"]
)
# Add exception handlers
app.add_exception_handler(RequestValidationError, validation_exception_handler)
app.add_exception_handler(Exception, general_exception_handler)

# Include routers
app.include_router(
    embeddings.router, 
    prefix="/api/embeddings", 
    tags=["embeddings"]
)

# Health check endpoints
@app.get("/health", response_model=HealthResponse)
async def health_check():
    """Health check endpoint"""
    return HealthResponse(
        status="healthy",
        model=settings.MODEL_NAME,
        dimension=embedding_model.dimension,
        device=settings.DEVICE
    )

@app.get("/")
async def root():
    """Root endpoint"""
    return {
        "service": settings.APP_NAME,
        "version": settings.APP_VERSION,
        "status": "running",
        "endpoints": {
            "health": "/health",
            "single_embedding": "/api/embeddings/single",
            "batch_embeddings": "/api/embeddings/batch"
        }
    }
from fastapi import APIRouter, HTTPException, status
from app.models.reranker import reranking_model
from app.config import get_settings
from app.schemas.requests import HealthResponse, DetailedHealthResponse
import logging
import psutil
import torch
from datetime import datetime

logger = logging.getLogger(__name__)
router = APIRouter()
settings = get_settings()

@router.get("/health", response_model=HealthResponse)
async def health_check():
    """Basic health check endpoint"""
    try:
        # Quick model check
        _ = reranking_model._model
        
        return HealthResponse(
            status="healthy",
            model=settings.MODEL_NAME,
            device=settings.DEVICE
        )
    except Exception as e:
        logger.error(f"Health check failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Service unhealthy"
        )

@router.get("/health/detailed", response_model=DetailedHealthResponse)
async def detailed_health_check():
    """Detailed health check with system metrics"""
    try:
        # Model info
        model_info = {
            "name": settings.MODEL_NAME,
            "max_length": settings.MAX_TEXT_LENGTH,
            "device": settings.DEVICE,
            "is_cuda_available": torch.cuda.is_available()
        }
        
        # System metrics
        memory = psutil.virtual_memory()
        cpu_percent = psutil.cpu_percent(interval=0.1)
        
        system_info = {
            "cpu_usage_percent": cpu_percent,
            "memory_total_gb": round(memory.total / (1024**3), 2),
            "memory_used_gb": round(memory.used / (1024**3), 2),
            "memory_percent": memory.percent,
            "timestamp": datetime.utcnow().isoformat()
        }
        
        # GPU info if available
        gpu_info = None
        if torch.cuda.is_available():
            gpu_info = {
                "gpu_count": torch.cuda.device_count(),
                "gpu_name": torch.cuda.get_device_name(0) if torch.cuda.device_count() > 0 else None,
                "gpu_memory_allocated_mb": round(torch.cuda.memory_allocated(0) / (1024**2), 2) if torch.cuda.device_count() > 0 else 0,
                "gpu_memory_reserved_mb": round(torch.cuda.memory_reserved(0) / (1024**2), 2) if torch.cuda.device_count() > 0 else 0
            }
        
        return DetailedHealthResponse(
            status="healthy",
            model=model_info,
            system=system_info,
            gpu=gpu_info,
            config={
                "max_batch_size": settings.MAX_BATCH_SIZE,
                "max_text_length": settings.MAX_TEXT_LENGTH,
                "max_pairs": settings.MAX_PAIRS
            }
        )
    except Exception as e:
        logger.error(f"Detailed health check failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail=str(e)
        )

@router.get("/health/ready")
async def readiness_check():
    """Kubernetes readiness probe endpoint"""
    try:
        # Test actual reranking
        test_scores = reranking_model.predict_scores([
            ("test query", "test passage")
        ])
        
        if len(test_scores) == 1:
            return {
                "status": "ready",
                "message": "Service is ready to accept requests"
            }
        else:
            raise Exception("Model output error")
    except Exception as e:
        logger.error(f"Readiness check failed: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Service not ready"
        )

@router.get("/health/live")
async def liveness_check():
    """Kubernetes liveness probe endpoint"""
    # Simple check that the service is running
    return {
        "status": "alive",
        "timestamp": datetime.utcnow().isoformat()
    }
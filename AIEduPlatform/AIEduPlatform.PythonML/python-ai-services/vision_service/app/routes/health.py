from fastapi import APIRouter, Depends
from pydantic import BaseModel
from typing import Dict, Any

from app.config import get_settings, Settings


router = APIRouter()


class HealthResponse(BaseModel):
    """Health check response model."""
    model_config = {"protected_namespaces": ()}
    status: str
    service: str
    version: str
    model_name: str
    gpu_enabled: bool


class ReadinessResponse(BaseModel):
    """Readiness check response model."""
    ready: bool
    details: Dict[str, Any]


@router.get("", response_model=HealthResponse)
@router.get("/", response_model=HealthResponse)
async def health_check(settings: Settings = Depends(get_settings)) -> HealthResponse:
    """Basic health check endpoint."""
    return HealthResponse(
        status="healthy",
        service=settings.app_name,
        version=settings.app_version,
        model_name=settings.model_name,
        gpu_enabled=settings.use_gpu
    )


@router.get("/ready", response_model=ReadinessResponse)
async def readiness_check(settings: Settings = Depends(get_settings)) -> ReadinessResponse:
    """Readiness check - verifies model is loaded and ready."""
    from app.main import get_analyzer
    
    analyzer = get_analyzer()
    is_ready = analyzer is not None
    
    details = {
        "model_loaded": is_ready,
        "model_name": settings.model_name
    }
    
    return ReadinessResponse(ready=is_ready, details=details)


@router.get("/live")
async def liveness_check() -> Dict[str, str]:
    """Liveness check - simple ping."""
    return {"status": "alive"}

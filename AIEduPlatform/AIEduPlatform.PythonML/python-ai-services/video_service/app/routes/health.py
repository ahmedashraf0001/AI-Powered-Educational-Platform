from fastapi import APIRouter
from pydantic import BaseModel
from typing import Optional
import httpx

router = APIRouter()


class HealthResponse(BaseModel):
    """Health check response model."""
    status: str
    service: str
    version: str
    vision_service: Optional[str] = None
    transcription_service: Optional[str] = None


@router.get("", response_model=HealthResponse)
@router.get("/", response_model=HealthResponse)
async def health_check() -> HealthResponse:
    """Check service health and upstream service reachability."""
    from app.main import get_analyzer
    from app.config import get_settings
    
    settings = get_settings()
    analyzer = get_analyzer()
    
    # Quick non-blocking checks against upstream services
    vision_status = "unknown"
    transcription_status = "unknown"
    
    try:
        async with httpx.AsyncClient(timeout=3.0) as client:
            try:
                r = await client.get(f"{settings.vision_service_url}/health")
                vision_status = "healthy" if r.status_code == 200 else f"status {r.status_code}"
            except Exception:
                vision_status = "unreachable"
            
            try:
                r = await client.get(f"{settings.transcription_service_url}/health")
                transcription_status = "healthy" if r.status_code == 200 else f"status {r.status_code}"
            except Exception:
                transcription_status = "unreachable"
    except Exception:
        pass
    
    return HealthResponse(
        status="healthy" if analyzer is not None else "initializing",
        service=settings.app_name,
        version=settings.app_version,
        vision_service=vision_status,
        transcription_service=transcription_status
    )


@router.get("/ready")
async def readiness_check():
    """Check if the service is ready to accept requests."""
    from app.main import get_analyzer
    
    analyzer = get_analyzer()
    if analyzer is None:
        return {"ready": False, "reason": "Analyzer not initialized"}
    
    return {"ready": True}

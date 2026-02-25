"""Routes module for Video Analysis service."""
from app.routes.health import router as health_router
from app.routes.video import router as video_router

__all__ = ["health_router", "video_router"]

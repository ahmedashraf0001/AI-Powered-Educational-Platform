"""Middleware module for Video Analysis service."""
from app.middleware.error_handler import add_error_handlers, VideoException, VideoProcessingError, ModelError

__all__ = ["add_error_handlers", "VideoException", "VideoProcessingError", "ModelError"]

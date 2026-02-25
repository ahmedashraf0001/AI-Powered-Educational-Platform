"""Schemas module for Video Analysis service."""
from app.schemas.requests import (
    VideoAnalysisRequest,
    VideoPathRequest,
    VideoAnalysisResponse,
    SegmentResponse,
    FrameAnalysisResponse
)

__all__ = [
    "VideoAnalysisRequest",
    "VideoPathRequest", 
    "VideoAnalysisResponse",
    "SegmentResponse",
    "FrameAnalysisResponse"
]

from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
import traceback


class TranscriptionException(Exception):
    """Base exception for Transcription service errors."""
    
    def __init__(self, message: str, status_code: int = 500, details: dict = None):
        self.message = message
        self.status_code = status_code
        self.details = details or {}
        super().__init__(self.message)


class AudioProcessingError(TranscriptionException):
    """Exception raised when audio processing fails."""
    
    def __init__(self, message: str, details: dict = None):
        super().__init__(message, status_code=400, details=details)


class UnsupportedFormatError(TranscriptionException):
    """Exception raised when audio format is not supported."""
    
    def __init__(self, message: str, details: dict = None):
        super().__init__(message, status_code=415, details=details)


class AudioTooLongError(TranscriptionException):
    """Exception raised when audio exceeds maximum duration."""
    
    def __init__(self, message: str, details: dict = None):
        super().__init__(message, status_code=413, details=details)


class ModelError(TranscriptionException):
    """Exception raised when model inference fails."""
    
    def __init__(self, message: str, details: dict = None):
        super().__init__(message, status_code=500, details=details)


def add_error_handlers(app: FastAPI) -> None:
    """Add custom error handlers to the FastAPI application."""
    
    @app.exception_handler(TranscriptionException)
    async def transcription_exception_handler(
        request: Request, 
        exc: TranscriptionException
    ) -> JSONResponse:
        return JSONResponse(
            status_code=exc.status_code,
            content={
                "error": exc.message,
                "type": type(exc).__name__,
                "details": exc.details
            }
        )
    
    @app.exception_handler(Exception)
    async def general_exception_handler(
        request: Request, 
        exc: Exception
    ) -> JSONResponse:
        traceback.print_exc()
        return JSONResponse(
            status_code=500,
            content={
                "error": "An unexpected error occurred",
                "type": "InternalServerError",
                "details": {"message": str(exc)}
            }
        )

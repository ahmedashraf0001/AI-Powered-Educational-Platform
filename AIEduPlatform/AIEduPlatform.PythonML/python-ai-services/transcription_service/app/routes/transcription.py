from fastapi import APIRouter, UploadFile, File, Depends, Query, Form
from typing import Optional, List
from pydantic import BaseModel, Field
import asyncio
import tempfile
import os

from app.config import get_settings, Settings
from app.middleware.error_handler import (
    AudioProcessingError, 
    UnsupportedFormatError, 
    AudioTooLongError,
    ModelError
)


router = APIRouter()


def get_transcriber():
    """Get the global transcriber instance."""
    from app.main import get_transcriber as get_model
    transcriber = get_model()
    if transcriber is None:
        raise ModelError("Audio transcriber not initialized")
    return transcriber


def validate_audio_format(filename: str, settings: Settings) -> None:
    """Validate that the audio format is supported."""
    if filename:
        ext = filename.rsplit(".", 1)[-1].lower()
        if ext not in settings.supported_formats:
            raise UnsupportedFormatError(
                f"Unsupported audio format: {ext}. "
                f"Supported formats: {', '.join(settings.supported_formats)}"
            )


class TranscriptionResponse(BaseModel):
    """Response model for transcription."""
    model_config = {"protected_namespaces": ()}
    
    text: str = Field(..., description="Transcribed text")
    language: Optional[str] = Field(None, description="Detected or specified language")
    language_probability: Optional[float] = Field(None, description="Language detection confidence")
    segments: List[dict] = Field(default_factory=list, description="Timestamped segments")
    llm_context: str = Field(..., description="Formatted text for LLM context")
    processing_time_ms: float = Field(..., description="Processing time in milliseconds")
    audio_duration_seconds: float = Field(..., description="Audio duration in seconds")
    model_name: str = Field(..., description="Model used for transcription")


class BatchTranscriptionResult(BaseModel):
    """Result for a single audio in batch processing."""
    index: int = Field(..., description="Index of the audio")
    success: bool = Field(..., description="Whether processing succeeded")
    text: Optional[str] = Field(None, description="Transcribed text")
    language: Optional[str] = Field(None, description="Detected language")
    llm_context: Optional[str] = Field(None, description="Formatted for LLM context")
    processing_time_ms: Optional[float] = Field(None, description="Processing time")
    error: Optional[str] = Field(None, description="Error message if failed")


class BatchTranscriptionResponse(BaseModel):
    """Response for batch audio transcription."""
    results: List[BatchTranscriptionResult] = Field(..., description="Results for each audio")
    total_files: int = Field(..., description="Total files processed")
    successful: int = Field(..., description="Number of successful transcriptions")
    failed: int = Field(..., description="Number of failed transcriptions")
    total_processing_time_ms: float = Field(..., description="Total processing time")


@router.post("/file", response_model=TranscriptionResponse)
async def transcribe_file(
    file: UploadFile = File(..., description="Audio file to transcribe"),
    language: Optional[str] = Form(None, description="Source language code (e.g., 'ar' for Arabic). Auto-detect if not provided"),
    task: str = Form("translate", description="'translate' outputs English (default), 'transcribe' keeps original language"),
    include_timestamps: bool = Form(True, description="Include timestamps"),
    include_metadata: bool = Form(False, description="Include metadata in LLM context"),
    settings: Settings = Depends(get_settings)
) -> TranscriptionResponse:
    """
    Transcribe an uploaded audio file.
    
    Accepts audio files in various formats (mp3, wav, flac, ogg, m4a, webm, mp4).
    Returns transcribed text suitable for use as LLM context.
    """
    # Validate format
    validate_audio_format(file.filename, settings)
    
    transcriber = get_transcriber()
    
    try:
        # Read audio content
        audio_content = await file.read()
        
        # Save to temporary file for processing
        ext = file.filename.rsplit(".", 1)[-1].lower() if file.filename else "wav"
        with tempfile.NamedTemporaryFile(suffix=f".{ext}", delete=False) as tmp_file:
            tmp_file.write(audio_content)
            tmp_path = tmp_file.name
        
        try:
            # Transcribe (offload to thread to avoid blocking event loop)
            result = await asyncio.to_thread(
                transcriber.transcribe,
                audio=tmp_path,
                sample_rate=settings.sample_rate,
                language=language,
                task=task,
                return_timestamps=include_timestamps,
                chunk_length_s=settings.chunk_length_seconds,
                batch_size=settings.batch_size
            )
            
            # Check duration
            if result.audio_duration_seconds > settings.max_audio_duration_seconds:
                raise AudioTooLongError(
                    f"Audio duration ({result.audio_duration_seconds:.1f}s) exceeds "
                    f"maximum allowed ({settings.max_audio_duration_seconds}s)"
                )
            
            return TranscriptionResponse(
                text=result.text,
                language=result.language,
                language_probability=result.language_probability,
                segments=[seg.to_dict() for seg in result.segments],
                llm_context=result.to_llm_context(include_timestamps=include_metadata),
                processing_time_ms=result.processing_time_ms,
                audio_duration_seconds=result.audio_duration_seconds,
                model_name=result.model_name
            )
        finally:
            # Cleanup temp file
            os.unlink(tmp_path)
            
    except (AudioProcessingError, UnsupportedFormatError, AudioTooLongError):
        raise
    except Exception as e:
        raise AudioProcessingError(f"Failed to transcribe audio: {str(e)}")


@router.post("/batch", response_model=BatchTranscriptionResponse)
async def transcribe_batch(
    files: List[UploadFile] = File(..., description="Audio files to transcribe"),
    global_language: Optional[str] = Form(None, description="Source language for all files. Auto-detect if not provided"),
    task: str = Form("translate", description="'translate' outputs English (default), 'transcribe' keeps original language"),
    include_timestamps: bool = Form(True, description="Include timestamps"),
    continue_on_error: bool = Form(True, description="Continue processing if a file fails"),
    settings: Settings = Depends(get_settings)
) -> BatchTranscriptionResponse:
    """
    Transcribe multiple audio files in batch via multipart form upload.

    Upload multiple audio files as multipart form data.
    Returns transcription results for each file.
    """
    import time
    start_time = time.time()

    transcriber = get_transcriber()
    results = []
    successful = 0
    failed = 0

    for idx, file in enumerate(files):
        try:
            # Validate format
            validate_audio_format(file.filename, settings)

            # Read file content
            audio_content = await file.read()

            ext = file.filename.rsplit(".", 1)[-1].lower() if file.filename else "wav"
            with tempfile.NamedTemporaryFile(suffix=f".{ext}", delete=False) as tmp_file:
                tmp_file.write(audio_content)
                audio_path = tmp_file.name

            try:
                # Transcribe (offload to thread to avoid blocking event loop)
                result = await asyncio.to_thread(
                    transcriber.transcribe,
                    audio=audio_path,
                    sample_rate=settings.sample_rate,
                    language=global_language,
                    task=task,
                    return_timestamps=include_timestamps
                )

                results.append(BatchTranscriptionResult(
                    index=idx,
                    success=True,
                    text=result.text,
                    language=result.language,
                    llm_context=result.to_llm_context(),
                    processing_time_ms=result.processing_time_ms,
                    error=None
                ))
                successful += 1

            finally:
                os.unlink(audio_path)

        except Exception as e:
            if not continue_on_error:
                raise AudioProcessingError(f"Batch processing failed at index {idx}: {str(e)}")

            results.append(BatchTranscriptionResult(
                index=idx,
                success=False,
                text=None,
                language=None,
                llm_context=None,
                processing_time_ms=None,
                error=str(e)
            ))
            failed += 1

    total_time = (time.time() - start_time) * 1000

    return BatchTranscriptionResponse(
        results=results,
        total_files=len(files),
        successful=successful,
        failed=failed,
        total_processing_time_ms=total_time
    )


@router.get("/supported-formats")
async def get_supported_formats(
    settings: Settings = Depends(get_settings)
) -> dict:
    """Get list of supported audio formats."""
    return {
        "supported_formats": settings.supported_formats,
        "max_duration_seconds": settings.max_audio_duration_seconds,
        "sample_rate": settings.sample_rate
    }


@router.get("/supported-languages")
async def get_supported_languages() -> dict:
    """Get list of supported languages for transcription and translation to English."""
    # Whisper supports these languages - all can be translated to English
    languages = {
        "ar": "Arabic (including Egyptian, Gulf, Levantine dialects)",
        "en": "English",
        "zh": "Chinese", 
        "de": "German",
        "es": "Spanish",
        "ru": "Russian",
        "ko": "Korean",
        "fr": "French",
        "ja": "Japanese",
        "pt": "Portuguese",
        "tr": "Turkish",
        "pl": "Polish",
        "ca": "Catalan",
        "nl": "Dutch",
        "sv": "Swedish",
        "it": "Italian",
        "id": "Indonesian",
        "hi": "Hindi",
        "fi": "Finnish",
        "vi": "Vietnamese",
        "he": "Hebrew",
        "uk": "Ukrainian",
        "el": "Greek",
        "ms": "Malay",
        "cs": "Czech",
        "ro": "Romanian",
        "da": "Danish",
        "hu": "Hungarian",
        "ta": "Tamil",
        "no": "Norwegian",
        "th": "Thai",
        "ur": "Urdu",
        "hr": "Croatian",
        "bg": "Bulgarian",
        "lt": "Lithuanian",
        "la": "Latin",
        "mi": "Maori",
        "ml": "Malayalam",
        "cy": "Welsh",
        "sk": "Slovak",
        "te": "Telugu",
        "fa": "Persian",
        "lv": "Latvian",
        "bn": "Bengali",
        "sr": "Serbian",
        "az": "Azerbaijani",
        "sl": "Slovenian",
        "kn": "Kannada",
        "et": "Estonian",
        "mk": "Macedonian",
        "br": "Breton",
        "eu": "Basque",
        "is": "Icelandic",
        "hy": "Armenian",
        "ne": "Nepali",
        "mn": "Mongolian",
        "bs": "Bosnian",
        "kk": "Kazakh",
        "sq": "Albanian",
        "sw": "Swahili",
        "gl": "Galician",
        "mr": "Marathi",
        "pa": "Punjabi",
        "si": "Sinhala",
        "km": "Khmer",
        "sn": "Shona",
        "yo": "Yoruba",
        "so": "Somali",
        "af": "Afrikaans",
        "oc": "Occitan",
        "ka": "Georgian",
        "be": "Belarusian",
        "tg": "Tajik",
        "sd": "Sindhi",
        "gu": "Gujarati",
        "am": "Amharic",
        "yi": "Yiddish",
        "lo": "Lao",
        "uz": "Uzbek",
        "fo": "Faroese",
        "ht": "Haitian Creole",
        "ps": "Pashto",
        "tk": "Turkmen",
        "nn": "Nynorsk",
        "mt": "Maltese",
        "sa": "Sanskrit",
        "lb": "Luxembourgish",
        "my": "Myanmar",
        "bo": "Tibetan",
        "tl": "Tagalog",
        "mg": "Malagasy",
        "as": "Assamese",
        "tt": "Tatar",
        "haw": "Hawaiian",
        "ln": "Lingala",
        "ha": "Hausa",
        "ba": "Bashkir",
        "jw": "Javanese",
        "su": "Sundanese"
    }
    
    return {
        "languages": languages,
        "auto_detect": True,
        "default_task": "translate",
        "output_language": "en",
        "note": "By default, all audio is translated to English. Use task='transcribe' to keep the original language.",
        "arabic_support": {
            "code": "ar",
            "dialects_supported": ["Egyptian (Masri)", "Gulf (Khaliji)", "Levantine (Shami)", "Modern Standard Arabic (MSA)", "Maghrebi"],
            "default_output": "English translation"
        }
    }

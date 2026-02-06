from typing import Optional, List, Dict, Any, Union
import torch
from transformers import AutoModelForSpeechSeq2Seq, AutoProcessor, pipeline
from dataclasses import dataclass
import numpy as np
import time
import io
import logging

logger = logging.getLogger(__name__)


@dataclass
class TranscriptionSegment:
    """A segment of transcribed audio with timestamps."""
    text: str
    start_time: float
    end_time: float
    
    def to_dict(self) -> dict:
        return {
            "text": self.text,
            "start_time": self.start_time,
            "end_time": self.end_time
        }


@dataclass
class TranscriptionResult:
    """Result from audio transcription."""
    text: str
    language: Optional[str]
    language_probability: Optional[float]
    segments: List[TranscriptionSegment]
    processing_time_ms: float
    audio_duration_seconds: float
    model_name: str
    
    def to_dict(self) -> dict:
        return {
            "text": self.text,
            "language": self.language,
            "language_probability": self.language_probability,
            "segments": [seg.to_dict() for seg in self.segments],
            "processing_time_ms": self.processing_time_ms,
            "audio_duration_seconds": self.audio_duration_seconds,
            "model_name": self.model_name
        }
    
    def to_llm_context(self, include_timestamps: bool = False) -> str:
        """Format the transcription as context for an LLM."""
        if include_timestamps and self.segments:
            lines = []
            for seg in self.segments:
                timestamp = f"[{seg.start_time:.2f}s - {seg.end_time:.2f}s]"
                lines.append(f"{timestamp} {seg.text}")
            return "\n".join(lines)
        return self.text


class AudioTranscriber:
    """Audio transcriber using Whisper model for speech-to-text."""
    
    def __init__(
        self,
        model_size: str = "base",
        use_gpu: bool = True,
        language: Optional[str] = None
    ):
        self.model_size = model_size
        self.device = "cuda" if use_gpu and torch.cuda.is_available() else "cpu"
        self.torch_dtype = torch.float16 if self.device == "cuda" else torch.float32
        self.default_language = language
        
        # Map model size to HuggingFace model name
        model_id = self._get_model_id(model_size)
        self.model_name = model_id
        
        logger.info(f"Loading transcription model: {model_id} on {self.device}")
        
        # Load model and processor
        self.model = AutoModelForSpeechSeq2Seq.from_pretrained(
            model_id,
            torch_dtype=self.torch_dtype,
            low_cpu_mem_usage=True,
            use_safetensors=True
        )
        self.model.to(self.device)
        self.model.eval()
        
        self.processor = AutoProcessor.from_pretrained(model_id)
        
        # Create pipeline for easier inference
        self.pipe = pipeline(
            "automatic-speech-recognition",
            model=self.model,
            tokenizer=self.processor.tokenizer,
            feature_extractor=self.processor.feature_extractor,
            torch_dtype=self.torch_dtype,
            device=self.device
        )
        
        logger.info("Transcription model loaded successfully")
    
    def _get_model_id(self, model_size: str) -> str:
        """Get the HuggingFace model ID for the given model size."""
        model_map = {
            "tiny": "openai/whisper-tiny",
            "base": "openai/whisper-base",
            "small": "openai/whisper-small",
            "medium": "openai/whisper-medium",
            "large": "openai/whisper-large-v3",
            "large-v2": "openai/whisper-large-v2",
            "large-v3": "openai/whisper-large-v3"
        }
        return model_map.get(model_size, "openai/whisper-base")
    
    def transcribe(
        self,
        audio: Union[np.ndarray, bytes, str],
        sample_rate: int = 16000,
        language: Optional[str] = None,
        task: str = "transcribe",
        return_timestamps: bool = True,
        chunk_length_s: int = 30,
        batch_size: int = 8
    ) -> TranscriptionResult:
        """
        Transcribe audio to text.
        
        Args:
            audio: Audio data as numpy array, bytes, or file path
            sample_rate: Sample rate of the audio (default 16000 Hz)
            language: Language code (e.g., 'en', 'es'). None for auto-detect
            task: 'transcribe' or 'translate' (to English)
            return_timestamps: Whether to return word/segment timestamps
            chunk_length_s: Length of audio chunks for processing
            batch_size: Batch size for processing chunks
            
        Returns:
            TranscriptionResult with transcribed text and metadata
        """
        start_time = time.time()
        
        # Prepare audio input
        audio_input = self._prepare_audio(audio, sample_rate)
        audio_duration = self._get_audio_duration(audio_input, sample_rate)
        
        # Build generation kwargs
        generate_kwargs = {
            "task": task,
        }
        
        # Use specified language or default
        effective_language = language or self.default_language
        if effective_language:
            generate_kwargs["language"] = effective_language
        
        # Run transcription
        with torch.no_grad():
            result = self.pipe(
                audio_input,
                return_timestamps=return_timestamps,
                chunk_length_s=chunk_length_s,
                batch_size=batch_size,
                generate_kwargs=generate_kwargs
            )
        
        # Parse results
        transcribed_text = result.get("text", "").strip()
        segments = self._parse_segments(result.get("chunks", []))
        
        # Detect language if not specified
        detected_language = effective_language
        language_probability = None
        
        processing_time_ms = (time.time() - start_time) * 1000
        
        return TranscriptionResult(
            text=transcribed_text,
            language=detected_language,
            language_probability=language_probability,
            segments=segments,
            processing_time_ms=processing_time_ms,
            audio_duration_seconds=audio_duration,
            model_name=self.model_name
        )
    
    def _prepare_audio(
        self, 
        audio: Union[np.ndarray, bytes, str],
        sample_rate: int
    ) -> Union[np.ndarray, str]:
        """Prepare audio input for the model."""
        if isinstance(audio, str):
            # File path - return as is, pipeline handles it
            return audio
        elif isinstance(audio, bytes):
            # Convert bytes to numpy array
            # For raw PCM data, assume float32 or int16
            try:
                import soundfile as sf
                audio_array, sr = sf.read(io.BytesIO(audio))
                if sr != sample_rate:
                    # Resample if needed
                    audio_array = self._resample(audio_array, sr, sample_rate)
                return audio_array
            except Exception:
                # Fallback: try to interpret as raw PCM
                audio_array = np.frombuffer(audio, dtype=np.float32)
                return audio_array
        elif isinstance(audio, np.ndarray):
            # Already numpy array
            return audio
        else:
            raise ValueError(f"Unsupported audio type: {type(audio)}")
    
    def _resample(
        self, 
        audio: np.ndarray, 
        orig_sr: int, 
        target_sr: int
    ) -> np.ndarray:
        """Resample audio to target sample rate."""
        try:
            import librosa
            return librosa.resample(audio, orig_sr=orig_sr, target_sr=target_sr)
        except ImportError:
            # Simple resampling fallback
            ratio = target_sr / orig_sr
            output_length = int(len(audio) * ratio)
            indices = np.linspace(0, len(audio) - 1, output_length)
            return np.interp(indices, np.arange(len(audio)), audio)
    
    def _get_audio_duration(
        self, 
        audio: Union[np.ndarray, str], 
        sample_rate: int
    ) -> float:
        """Get audio duration in seconds."""
        if isinstance(audio, np.ndarray):
            return len(audio) / sample_rate
        elif isinstance(audio, str):
            try:
                import soundfile as sf
                info = sf.info(audio)
                return info.duration
            except Exception:
                return 0.0
        return 0.0
    
    def _parse_segments(
        self, 
        chunks: List[Dict[str, Any]]
    ) -> List[TranscriptionSegment]:
        """Parse chunk results into TranscriptionSegment objects."""
        segments = []
        for chunk in chunks:
            timestamp = chunk.get("timestamp", (0.0, 0.0))
            if timestamp and len(timestamp) >= 2:
                start_time = timestamp[0] if timestamp[0] is not None else 0.0
                end_time = timestamp[1] if timestamp[1] is not None else 0.0
            else:
                start_time = 0.0
                end_time = 0.0
            
            text = chunk.get("text", "").strip()
            if text:
                segments.append(TranscriptionSegment(
                    text=text,
                    start_time=start_time,
                    end_time=end_time
                ))
        return segments
    
    def transcribe_batch(
        self,
        audio_files: List[str],
        language: Optional[str] = None,
        task: str = "transcribe",
        return_timestamps: bool = True
    ) -> List[TranscriptionResult]:
        """
        Transcribe multiple audio files.
        
        Args:
            audio_files: List of file paths to audio files
            language: Language code for all files (None for auto-detect)
            task: 'transcribe' or 'translate'
            return_timestamps: Whether to return timestamps
            
        Returns:
            List of TranscriptionResult objects
        """
        results = []
        for audio_path in audio_files:
            try:
                result = self.transcribe(
                    audio=audio_path,
                    language=language,
                    task=task,
                    return_timestamps=return_timestamps
                )
                results.append(result)
            except Exception as e:
                logger.error(f"Failed to transcribe {audio_path}: {str(e)}")
                # Add error result
                results.append(TranscriptionResult(
                    text="",
                    language=None,
                    language_probability=None,
                    segments=[],
                    processing_time_ms=0.0,
                    audio_duration_seconds=0.0,
                    model_name=self.model_name
                ))
        return results

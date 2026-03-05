from typing import Optional, List, Dict, Any, Union
import torch
from TTS.api import TTS
from dataclasses import dataclass, field
import numpy as np
import time
import io
import os
import base64
import tempfile
import logging
from pydub import AudioSegment
import subprocess
import shutil

logger = logging.getLogger(__name__)


@dataclass
class VoiceInfo:
    """Information about an available voice."""
    voice_id: str
    name: str
    description: Optional[str] = None
    gender: Optional[str] = None
    languages: List[str] = field(default_factory=lambda: ["en"])
    recommended_for_teacher: bool = False
    recommended_for_student: bool = False
    preview_url: Optional[str] = None
    
    def to_dict(self) -> dict:
        return {
            "voice_id": self.voice_id,
            "name": self.name,
            "description": self.description,
            "gender": self.gender,
            "languages": self.languages,
            "recommended_for_teacher": self.recommended_for_teacher,
            "recommended_for_student": self.recommended_for_student,
            "preview_url": self.preview_url
        }


@dataclass
class DialogueVoiceConfiguration:
    """Voice configuration for teacher and student."""
    teacher_voice_id: str = "Damien Black"  # Natural male voice
    student_voice_id: str = "Daisy Studious"  # Natural female voice
    teacher_speed: float = 1.0
    student_speed: float = 1.0
    teacher_voice_name: Optional[str] = "Male - Calm Professional"
    student_voice_name: Optional[str] = "Female - Curious Learner"
    
    def to_dict(self) -> dict:
        return {
            "teacher_voice_id": self.teacher_voice_id,
            "student_voice_id": self.student_voice_id,
            "teacher_speed": self.teacher_speed,
            "student_speed": self.student_speed,
            "teacher_voice_name": self.teacher_voice_name,
            "student_voice_name": self.student_voice_name
        }


@dataclass
class DialogueTurn:
    """A single turn in a dialogue."""
    speaker: str  # "teacher" or "student"
    text: str
    
    def to_dict(self) -> dict:
        return {
            "speaker": self.speaker,
            "text": self.text
        }


@dataclass
class TeacherStudentDialogue:
    """A complete teacher-student dialogue."""
    turns: List[DialogueTurn]
    topic: Optional[str] = None
    
    def to_dict(self) -> dict:
        return {
            "turns": [turn.to_dict() for turn in self.turns],
            "topic": self.topic
        }


@dataclass
class TurnTimestamp:
    """Timestamp for a dialogue turn."""
    turn_index: int
    speaker: str
    text: str
    start_time: float
    end_time: float
    
    @property
    def duration(self) -> float:
        return self.end_time - self.start_time
    
    def to_dict(self) -> dict:
        return {
            "turn_index": self.turn_index,
            "speaker": self.speaker,
            "text": self.text,
            "start_time": self.start_time,
            "end_time": self.end_time,
            "duration": self.duration
        }


@dataclass
class VoicePreviewResult:
    """Result of a single voice preview generation."""
    voice_id: str
    name: str
    description: Optional[str] = None
    gender: Optional[str] = None
    languages: List[str] = field(default_factory=lambda: ["en"])
    recommended_for_teacher: bool = False
    recommended_for_student: bool = False
    sample_text: str = ""
    audio_base64: Optional[str] = None
    format: str = "mp3"
    duration_seconds: float = 0.0
    file_size_bytes: int = 0
    sample_rate: int = 24000
    success: bool = True
    error_message: Optional[str] = None

    def to_dict(self) -> dict:
        return {
            "voice_id": self.voice_id,
            "name": self.name,
            "description": self.description,
            "gender": self.gender,
            "languages": self.languages,
            "recommended_for_teacher": self.recommended_for_teacher,
            "recommended_for_student": self.recommended_for_student,
            "sample_text": self.sample_text,
            "audio_base64": self.audio_base64,
            "format": self.format,
            "duration_seconds": self.duration_seconds,
            "file_size_bytes": self.file_size_bytes,
            "sample_rate": self.sample_rate,
            "success": self.success,
            "error_message": self.error_message
        }


@dataclass
class DialogueAudioResult:
    """Result of dialogue audio generation."""
    success: bool
    error_message: Optional[str] = None
    file_path: Optional[str] = None
    audio_data: Optional[bytes] = None
    format: str = "mp3"
    duration_seconds: float = 0.0
    file_size_bytes: int = 0
    processing_time_ms: float = 0.0
    turn_timestamps: List[TurnTimestamp] = field(default_factory=list)
    
    def to_dict(self) -> dict:
        return {
            "success": self.success,
            "error_message": self.error_message,
            "file_path": self.file_path,
            "format": self.format,
            "duration_seconds": self.duration_seconds,
            "file_size_bytes": self.file_size_bytes,
            "processing_time_ms": self.processing_time_ms,
            "turn_timestamps": [t.to_dict() for t in self.turn_timestamps]
        }


class AudioSynthesizer:
    """Text-to-speech synthesizer using XTTS v2 for natural, human-like dialogue audio."""
    
    # Available speakers from the XTTS v2 multi-dataset training set.
    # These voices are natural-sounding with breathing, pauses, and human cadence.
    AVAILABLE_VOICES = {
        # ── Male voices ───────────────────────────────────────
        "Damien Black": VoiceInfo(
            "Damien Black", "Male - Calm Professional",
            "Smooth, professional male voice with natural cadence and breathing",
            "male", ["en"], True, False),
        "Craig Gutsy": VoiceInfo(
            "Craig Gutsy", "Male - Confident Speaker",
            "Bold, engaging male voice with natural warmth",
            "male", ["en"], True, False),
        "Gilberto Mathias": VoiceInfo(
            "Gilberto Mathias", "Male - Warm Mentor",
            "Warm, encouraging male voice ideal for explanations",
            "male", ["en"], True, False),
        "Viktor Mansen": VoiceInfo(
            "Viktor Mansen", "Male - Mature Scholar",
            "Mature, authoritative male voice with measured pacing",
            "male", ["en"], True, False),
        "Andrew Chipper": VoiceInfo(
            "Andrew Chipper", "Male - Friendly Guide",
            "Upbeat, friendly male voice with natural energy",
            "male", ["en"], True, True),
        "Zacharie Aimilios": VoiceInfo(
            "Zacharie Aimilios", "Male - Articulate",
            "Clear, articulate male voice with precise diction",
            "male", ["en"], True, True),
        # ── Female voices ─────────────────────────────────────
        "Daisy Studious": VoiceInfo(
            "Daisy Studious", "Female - Curious Learner",
            "Curious, studious female voice with natural tone and inflection",
            "female", ["en"], False, True),
        "Sofia Hellen": VoiceInfo(
            "Sofia Hellen", "Female - Warm Professional",
            "Professional, warm female voice suited for teaching",
            "female", ["en"], True, False),
        "Gracie Wise": VoiceInfo(
            "Gracie Wise", "Female - Thoughtful Speaker",
            "Thoughtful, articulate female voice with natural pauses",
            "female", ["en"], False, True),
        "Claribel Dervla": VoiceInfo(
            "Claribel Dervla", "Female - Natural Narrator",
            "Natural, engaging female voice with expressive delivery",
            "female", ["en"], True, True),
        "Brenda Stern": VoiceInfo(
            "Brenda Stern", "Female - Authoritative",
            "Clear, authoritative female voice for confident delivery",
            "female", ["en"], True, False),
        "Annmarie Nele": VoiceInfo(
            "Annmarie Nele", "Female - Young & Friendly",
            "Young, friendly female voice with natural enthusiasm",
            "female", ["en"], False, True),
    }
    
    def __init__(
        self,
        model_name: str = "tts_models/multilingual/multi-dataset/xtts_v2",
        use_gpu: bool = True
    ):
        self.model_name = model_name
        self.device = "cuda" if use_gpu and torch.cuda.is_available() else "cpu"
        
        logger.info(f"Loading TTS model: {model_name} on {self.device}")
        
        # Initialize TTS model
        self.tts = TTS(model_name=model_name).to(self.device)
        
        logger.info(f"TTS model loaded successfully (GPU: {self.device == 'cuda'})")
        
        # Log available speakers for debugging
        if hasattr(self.tts, 'speakers') and self.tts.speakers:
            logger.info(f"Available speakers in model: {len(self.tts.speakers)}")
    
    @property
    def _is_multilingual(self) -> bool:
        """Check if the loaded model requires a language parameter."""
        return hasattr(self.tts, 'languages') and self.tts.languages is not None
    
    def get_available_voices(self) -> List[VoiceInfo]:
        """Get list of available voices."""
        return list(self.AVAILABLE_VOICES.values())
    
    def get_default_voice_configuration(self) -> DialogueVoiceConfiguration:
        """Get default voice configuration for dialogues."""
        return DialogueVoiceConfiguration(
            teacher_voice_id="Damien Black",
            student_voice_id="Daisy Studious",
            teacher_speed=0.95,  # Slightly slower for clarity
            student_speed=1.0,
            teacher_voice_name="Male - Calm Professional",
            student_voice_name="Female - Curious Learner"
        )
    
    def _apply_speed_stretch(self, wav_path: str, speed: float) -> None:
        """Apply speed change to a WAV file in-place using ffmpeg's atempo filter.

        XTTS v2's built-in ``speed`` parameter is unreliable in TTS 0.22.0 —
        the GPT decoder ignores it in practice.  This method applies the
        requested speed via ffmpeg's ``atempo`` audio filter, which is
        specifically designed for speech tempo changes: it preserves pitch
        and does not produce phase-vocoder echo artefacts.

        Args:
            wav_path: Path to an existing WAV file. Modified in-place.
            speed:    Speed multiplier. >1.0 = faster, <1.0 = slower.
                      Values within 0.01 of 1.0 are treated as a no-op.
        """
        if abs(speed - 1.0) < 0.01:
            return  # Nothing to do

        tmp_out = wav_path + ".tempo.wav"
        try:
            subprocess.run(
                [
                    "ffmpeg", "-y", "-i", wav_path,
                    "-filter:a", f"atempo={speed}",
                    tmp_out,
                ],
                capture_output=True,
                check=True,
            )
            shutil.move(tmp_out, wav_path)
            logger.debug("Applied atempo %.2fx to %s", speed, wav_path)
        except subprocess.CalledProcessError as e:
            logger.error("ffmpeg atempo failed: %s", e.stderr.decode())
            # If stretch fails, keep the original (normal speed) file
            if os.path.exists(tmp_out):
                os.unlink(tmp_out)

    def synthesize_text(
        self,
        text: str,
        speaker_id: str = "Damien Black",
        speed: float = 1.0,
        output_path: Optional[str] = None
    ) -> tuple[np.ndarray, int]:
        """
        Synthesize speech from text.
        
        Args:
            text: Text to synthesize
            speaker_id: Voice/speaker name to use
            speed: Speech speed multiplier
            output_path: Optional path to save the audio
            
        Returns:
            Tuple of (audio array, sample rate)
        """
        # Build common kwargs for multilingual models (XTTS v2 requires language)
        tts_kwargs = {}
        if self._is_multilingual:
            tts_kwargs["language"] = "en"
        
        # Generate speech
        if output_path:
            self.tts.tts_to_file(
                text=text,
                speaker=speaker_id,
                file_path=output_path,
                speed=speed,
                **tts_kwargs
            )
            # Load the file to get audio data
            audio = AudioSegment.from_file(output_path)
            samples = np.array(audio.get_array_of_samples())
            return samples, audio.frame_rate
        else:
            # Generate to memory
            wav = self.tts.tts(
                text=text,
                speaker=speaker_id,
                speed=speed,
                **tts_kwargs
            )
            # Get sample rate from model config
            sample_rate = self.tts.synthesizer.output_sample_rate
            return np.array(wav), sample_rate
    
    def generate_dialogue_audio(
        self,
        dialogue: TeacherStudentDialogue,
        voice_config: Optional[DialogueVoiceConfiguration] = None,
        output_format: str = "mp3",
        sample_rate: int = 24000,
        include_pauses: bool = True,
        pause_duration_ms: int = 500,
        pause_multiplier: float = 1.0,
        normalize_audio: bool = True,
        output_file_path: Optional[str] = None
    ) -> DialogueAudioResult:
        """
        Generate audio from a teacher-student dialogue.
        
        Args:
            dialogue: The dialogue to convert to audio
            voice_config: Voice configuration for speakers
            output_format: Output audio format (mp3, wav, ogg)
            sample_rate: Audio sample rate
            include_pauses: Whether to add pauses between turns
            pause_duration_ms: Base pause duration in milliseconds
            pause_multiplier: Multiplier for pause durations
            normalize_audio: Whether to normalize audio levels
            output_file_path: Optional output file path
            
        Returns:
            DialogueAudioResult with the generated audio
        """
        start_time = time.time()
        
        if voice_config is None:
            voice_config = self.get_default_voice_configuration()
        
        try:
            # Generate audio for each turn
            audio_segments: List[AudioSegment] = []
            turn_timestamps: List[TurnTimestamp] = []
            current_time = 0.0
            
            actual_pause_ms = int(pause_duration_ms * pause_multiplier)
            silence = AudioSegment.silent(duration=actual_pause_ms)
            
            for idx, turn in enumerate(dialogue.turns):
                # Determine voice settings based on speaker
                if turn.speaker.lower() == "teacher":
                    speaker_id = voice_config.teacher_voice_id
                    speed = voice_config.teacher_speed
                else:
                    speaker_id = voice_config.student_voice_id
                    speed = voice_config.student_speed
                
                # Generate audio for this turn
                with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
                    tmp_path = tmp.name
                
                try:
                    # Build kwargs for multilingual models
                    tts_kwargs = {}
                    if self._is_multilingual:
                        tts_kwargs["language"] = "en"
                    
                    # Generate at normal speed — XTTS v2's speed param is
                    # unreliable; speed is applied below via time-stretch.
                    self.tts.tts_to_file(
                        text=turn.text,
                        speaker=speaker_id,
                        file_path=tmp_path,
                        speed=1.0,
                        **tts_kwargs
                    )
                    self._apply_speed_stretch(tmp_path, speed)

                    # Load the generated audio
                    turn_audio = AudioSegment.from_wav(tmp_path)
                    
                    # Record timestamp
                    turn_start = current_time
                    turn_duration = len(turn_audio) / 1000.0  # Convert ms to seconds
                    turn_end = turn_start + turn_duration
                    
                    turn_timestamps.append(TurnTimestamp(
                        turn_index=idx,
                        speaker=turn.speaker,
                        text=turn.text,
                        start_time=turn_start,
                        end_time=turn_end
                    ))
                    
                    # Add to segments
                    audio_segments.append(turn_audio)
                    current_time = turn_end
                    
                    # Add pause between turns (except after last turn)
                    if include_pauses and idx < len(dialogue.turns) - 1:
                        audio_segments.append(silence)
                        current_time += actual_pause_ms / 1000.0
                        
                finally:
                    # Cleanup temp file
                    if os.path.exists(tmp_path):
                        os.unlink(tmp_path)
            
            # Combine all segments
            combined_audio = audio_segments[0]
            for segment in audio_segments[1:]:
                combined_audio += segment
            
            # Normalize if requested
            if normalize_audio:
                combined_audio = combined_audio.normalize()
            
            # Set sample rate
            combined_audio = combined_audio.set_frame_rate(sample_rate)
            
            # Export to desired format
            if output_file_path:
                combined_audio.export(output_file_path, format=output_format)
                audio_data = None
                file_size = os.path.getsize(output_file_path)
            else:
                # Export to bytes
                buffer = io.BytesIO()
                combined_audio.export(buffer, format=output_format)
                audio_data = buffer.getvalue()
                file_size = len(audio_data)
            
            processing_time = (time.time() - start_time) * 1000
            
            return DialogueAudioResult(
                success=True,
                file_path=output_file_path,
                audio_data=audio_data,
                format=output_format,
                duration_seconds=len(combined_audio) / 1000.0,
                file_size_bytes=file_size,
                processing_time_ms=processing_time,
                turn_timestamps=turn_timestamps
            )
            
        except Exception as e:
            logger.error(f"Failed to generate dialogue audio: {str(e)}")
            return DialogueAudioResult(
                success=False,
                error_message=str(e),
                processing_time_ms=(time.time() - start_time) * 1000
            )
    
    # Default sample sentences per voice style for previews
    PREVIEW_SENTENCES = {
        "teacher": "Welcome to today's lesson. Let's explore this topic together and understand the key concepts.",
        "student": "That's a great explanation! Could you go over that last part one more time?",
        "neutral": "This is a sample of my voice. You can use me for either teacher or student roles."
    }

    def generate_voice_preview(
        self,
        voice_id: Optional[str] = None,
        sample_text: Optional[str] = None,
        output_format: str = "mp3",
        sample_rate: int = 24000
    ) -> List[VoicePreviewResult]:
        """
        Generate audio preview samples for one or all voices.

        Args:
            voice_id: Specific voice to preview, or None for all voices.
            sample_text: Custom text to speak. If None, uses a role-appropriate default.
            output_format: Audio format for the preview clip.
            sample_rate: Sample rate for the preview clip.

        Returns:
            List of VoicePreviewResult with base64 audio and metadata.
        """
        voices_to_preview = (
            {voice_id: self.AVAILABLE_VOICES[voice_id]}
            if voice_id and voice_id in self.AVAILABLE_VOICES
            else self.AVAILABLE_VOICES
        )

        results: List[VoicePreviewResult] = []

        for vid, voice_info in voices_to_preview.items():
            # Pick appropriate sample text
            if sample_text:
                text = sample_text
            elif voice_info.recommended_for_teacher and not voice_info.recommended_for_student:
                text = self.PREVIEW_SENTENCES["teacher"]
            elif voice_info.recommended_for_student and not voice_info.recommended_for_teacher:
                text = self.PREVIEW_SENTENCES["student"]
            else:
                text = self.PREVIEW_SENTENCES["neutral"]

            try:
                with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
                    tmp_path = tmp.name

                try:
                    # Build kwargs for multilingual models
                    tts_kwargs = {}
                    if self._is_multilingual:
                        tts_kwargs["language"] = "en"
                    
                    self.tts.tts_to_file(
                        text=text,
                        speaker=vid,
                        file_path=tmp_path,
                        speed=1.0,
                        **tts_kwargs
                    )

                    audio = AudioSegment.from_wav(tmp_path)
                    audio = audio.set_frame_rate(sample_rate)

                    buffer = io.BytesIO()
                    audio.export(buffer, format=output_format)
                    audio_bytes = buffer.getvalue()

                    results.append(VoicePreviewResult(
                        voice_id=vid,
                        name=voice_info.name,
                        description=voice_info.description,
                        gender=voice_info.gender,
                        languages=voice_info.languages,
                        recommended_for_teacher=voice_info.recommended_for_teacher,
                        recommended_for_student=voice_info.recommended_for_student,
                        sample_text=text,
                        audio_base64=base64.b64encode(audio_bytes).decode("utf-8"),
                        format=output_format,
                        duration_seconds=len(audio) / 1000.0,
                        file_size_bytes=len(audio_bytes),
                        sample_rate=sample_rate,
                        success=True
                    ))
                finally:
                    if os.path.exists(tmp_path):
                        os.unlink(tmp_path)

            except Exception as e:
                logger.error(f"Failed to generate preview for voice {vid}: {e}")
                results.append(VoicePreviewResult(
                    voice_id=vid,
                    name=voice_info.name,
                    description=voice_info.description,
                    gender=voice_info.gender,
                    languages=voice_info.languages,
                    recommended_for_teacher=voice_info.recommended_for_teacher,
                    recommended_for_student=voice_info.recommended_for_student,
                    sample_text=text,
                    success=False,
                    error_message=str(e)
                ))

        return results

    def synthesize_single(
        self,
        text: str,
        voice_id: str = "Damien Black",
        speed: float = 1.0,
        output_format: str = "mp3",
        output_file_path: Optional[str] = None
    ) -> Dict[str, Any]:
        """
        Synthesize a single text to speech.
        
        Args:
            text: Text to synthesize
            voice_id: Voice name to use
            speed: Speech speed
            output_format: Output format
            output_file_path: Optional output path
            
        Returns:
            Dictionary with audio data and metadata
        """
        start_time = time.time()
        
        try:
            with tempfile.NamedTemporaryFile(suffix=".wav", delete=False) as tmp:
                tmp_path = tmp.name
            
            try:
                # Build kwargs for multilingual models
                tts_kwargs = {}
                if self._is_multilingual:
                    tts_kwargs["language"] = "en"
                
                # Generate at normal speed — XTTS v2's speed param is
                # unreliable; speed is applied below via time-stretch.
                self.tts.tts_to_file(
                    text=text,
                    speaker=voice_id,
                    file_path=tmp_path,
                    speed=1.0,
                    **tts_kwargs
                )
                self._apply_speed_stretch(tmp_path, speed)

                # Load and convert
                audio = AudioSegment.from_wav(tmp_path)
                
                if output_file_path:
                    audio.export(output_file_path, format=output_format)
                    audio_data = None
                    file_size = os.path.getsize(output_file_path)
                else:
                    buffer = io.BytesIO()
                    audio.export(buffer, format=output_format)
                    audio_data = buffer.getvalue()
                    file_size = len(audio_data)
                
                return {
                    "success": True,
                    "audio_data": audio_data,
                    "file_path": output_file_path,
                    "format": output_format,
                    "duration_seconds": len(audio) / 1000.0,
                    "file_size_bytes": file_size,
                    "processing_time_ms": (time.time() - start_time) * 1000
                }
            finally:
                if os.path.exists(tmp_path):
                    os.unlink(tmp_path)
                    
        except Exception as e:
            logger.error(f"Failed to synthesize text: {str(e)}")
            return {
                "success": False,
                "error_message": str(e),
                "processing_time_ms": (time.time() - start_time) * 1000
            }

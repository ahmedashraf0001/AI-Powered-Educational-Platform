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
    teacher_voice_id: str = "p286"  # Male voice
    student_voice_id: str = "p270"  # Female voice
    teacher_speed: float = 1.0
    student_speed: float = 1.0
    teacher_voice_name: Optional[str] = "Teacher (Male)"
    student_voice_name: Optional[str] = "Student (Female)"
    
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
    sample_rate: int = 22050
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
    """Text-to-speech synthesizer for generating dialogue audio."""
    
    # Available speaker IDs from VCTK dataset used by many TTS models
    AVAILABLE_VOICES = {
        # Male voices
        "p267": VoiceInfo("p267", "Male Teacher (British)", "Clear, authoritative male voice", "male", ["en"], True, False),
        "p247": VoiceInfo("p247", "Male Professor (British)", "Mature, scholarly male voice", "male", ["en"], True, False),
        "p263": VoiceInfo("p263", "Male Instructor (British)", "Friendly male voice", "male", ["en"], True, False),
        "p274": VoiceInfo("p274", "Male Mentor (British)", "Warm, encouraging male voice", "male", ["en"], True, False),
        "p286": VoiceInfo("p286", "Male Guide (British)", "Patient, clear male voice", "male", ["en"], True, False),
        # Female voices
        "p230": VoiceInfo("p230", "Female Student (British)", "Young, curious female voice", "female", ["en"], False, True),
        "p231": VoiceInfo("p231", "Female Learner (British)", "Engaged, eager female voice", "female", ["en"], False, True),
        "p239": VoiceInfo("p239", "Female Pupil (British)", "Thoughtful female voice", "female", ["en"], False, True),
        "p270": VoiceInfo("p270", "Female Teacher (British)", "Professional female voice", "female", ["en"], True, False),
        "p306": VoiceInfo("p306", "Female Assistant (British)", "Helpful, clear female voice", "female", ["en"], True, True),
        # Neutral/Other
        "p225": VoiceInfo("p225", "Neutral Voice 1", "Clear, neutral voice", "neutral", ["en"], True, True),
        "p226": VoiceInfo("p226", "Neutral Voice 2", "Balanced, neutral voice", "neutral", ["en"], True, True),
    }
    
    def __init__(
        self,
        model_name: str = "tts_models/en/vctk/vits",
        use_gpu: bool = True
    ):
        self.model_name = model_name
        self.device = "cuda" if use_gpu and torch.cuda.is_available() else "cpu"
        
        logger.info(f"Loading TTS model: {model_name} on {self.device}")
        
        # Initialize TTS model
        self.tts = TTS(model_name=model_name).to(self.device)
        
        logger.info(f"TTS model loaded successfully (GPU: {self.device == 'cuda'})")
    
    def get_available_voices(self) -> List[VoiceInfo]:
        """Get list of available voices."""
        return list(self.AVAILABLE_VOICES.values())
    
    def get_default_voice_configuration(self) -> DialogueVoiceConfiguration:
        """Get default voice configuration for dialogues."""
        return DialogueVoiceConfiguration(
            teacher_voice_id="p267",
            student_voice_id="p230",
            teacher_speed=0.95,  # Slightly slower for clarity
            student_speed=1.0,
            teacher_voice_name="Male Teacher (British)",
            student_voice_name="Female Student (British)"
        )
    
    def synthesize_text(
        self,
        text: str,
        speaker_id: str = "p267",
        speed: float = 1.0,
        output_path: Optional[str] = None
    ) -> tuple[np.ndarray, int]:
        """
        Synthesize speech from text.
        
        Args:
            text: Text to synthesize
            speaker_id: Voice/speaker ID to use
            speed: Speech speed multiplier
            output_path: Optional path to save the audio
            
        Returns:
            Tuple of (audio array, sample rate)
        """
        # Generate speech
        if output_path:
            self.tts.tts_to_file(
                text=text,
                speaker=speaker_id,
                file_path=output_path,
                speed=speed
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
                speed=speed
            )
            # Get sample rate from model config
            sample_rate = self.tts.synthesizer.output_sample_rate
            return np.array(wav), sample_rate
    
    def generate_dialogue_audio(
        self,
        dialogue: TeacherStudentDialogue,
        voice_config: Optional[DialogueVoiceConfiguration] = None,
        output_format: str = "mp3",
        sample_rate: int = 22050,
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
                    self.tts.tts_to_file(
                        text=turn.text,
                        speaker=speaker_id,
                        file_path=tmp_path,
                        speed=speed
                    )
                    
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
        sample_rate: int = 22050
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
                    self.tts.tts_to_file(
                        text=text,
                        speaker=vid,
                        file_path=tmp_path,
                        speed=1.0
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
        voice_id: str = "p267",
        speed: float = 1.0,
        output_format: str = "mp3",
        output_file_path: Optional[str] = None
    ) -> Dict[str, Any]:
        """
        Synthesize a single text to speech.
        
        Args:
            text: Text to synthesize
            voice_id: Voice ID to use
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
                # Generate audio
                self.tts.tts_to_file(
                    text=text,
                    speaker=voice_id,
                    file_path=tmp_path,
                    speed=speed
                )
                
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

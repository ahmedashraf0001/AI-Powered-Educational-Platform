import os
import asyncio
from typing import Optional, List, Dict, Any
from dataclasses import dataclass, field
import time
import tempfile
import subprocess
import json
import cv2
import httpx
import logging

logger = logging.getLogger(__name__)


@dataclass
class FrameAnalysis:
    """Analysis result for a single video frame."""
    timestamp_seconds: float
    description: str
    frame_number: int
    
    def to_dict(self) -> dict:
        return {
            "timestamp_seconds": self.timestamp_seconds,
            "description": self.description,
            "frame_number": self.frame_number
        }


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
class VideoSegment:
    """A combined segment of video with visual and audio information."""
    start_time: float
    end_time: float
    visual_description: Optional[str]
    transcript: Optional[str]
    
    def to_dict(self) -> dict:
        return {
            "start_time": self.start_time,
            "end_time": self.end_time,
            "visual_description": self.visual_description,
            "transcript": self.transcript
        }
    
    def to_llm_context(self) -> str:
        """Format this segment as context for an LLM."""
        time_range = f"[{self._format_time(self.start_time)} - {self._format_time(self.end_time)}]"
        lines = [time_range]
        if self.visual_description:
            lines.append(f"  Visual: {self.visual_description}")
        if self.transcript:
            lines.append(f"  Audio: \"{self.transcript}\"")
        return "\n".join(lines)
    
    @staticmethod
    def _format_time(seconds: float) -> str:
        """Format seconds as MM:SS or HH:MM:SS."""
        hours = int(seconds // 3600)
        minutes = int((seconds % 3600) // 60)
        secs = int(seconds % 60)
        if hours > 0:
            return f"{hours:02d}:{minutes:02d}:{secs:02d}"
        return f"{minutes:02d}:{secs:02d}"


@dataclass
class VideoAnalysisResult:
    """Complete result from video analysis."""
    segments: List[VideoSegment]
    full_transcript: str
    frame_analyses: List[FrameAnalysis]
    video_duration_seconds: float
    processing_time_ms: float
    video_dimensions: tuple
    fps: float
    has_audio: bool
    frames_analyzed: int
    
    def to_dict(self) -> dict:
        return {
            "segments": [seg.to_dict() for seg in self.segments],
            "full_transcript": self.full_transcript,
            "frame_analyses": [fa.to_dict() for fa in self.frame_analyses],
            "video_duration_seconds": self.video_duration_seconds,
            "processing_time_ms": self.processing_time_ms,
            "video_dimensions": {
                "width": self.video_dimensions[0],
                "height": self.video_dimensions[1]
            },
            "fps": self.fps,
            "has_audio": self.has_audio,
            "frames_analyzed": self.frames_analyzed
        }
    
    def to_llm_context(
        self, 
        include_timestamps: bool = True,
        include_visual: bool = True,
        include_audio: bool = True,
        summary_format: bool = False
    ) -> str:
        """
        Format the video analysis as context for an LLM.
        
        Args:
            include_timestamps: Include timestamp markers
            include_visual: Include visual descriptions
            include_audio: Include audio transcriptions
            summary_format: Use a more compact summary format
        """
        if summary_format:
            return self._to_summary_context(include_visual, include_audio)
        
        lines = []
        lines.append(f"[Video Analysis - Duration: {VideoSegment._format_time(self.video_duration_seconds)}]")
        lines.append("")
        
        for segment in self.segments:
            if include_timestamps:
                time_range = f"[{VideoSegment._format_time(segment.start_time)} - {VideoSegment._format_time(segment.end_time)}]"
                lines.append(time_range)
            
            if include_visual and segment.visual_description:
                lines.append(f"  Visual: {segment.visual_description}")
            
            if include_audio and segment.transcript:
                lines.append(f"  Audio: \"{segment.transcript}\"")
            
            lines.append("")
        
        return "\n".join(lines).strip()
    
    def _to_summary_context(self, include_visual: bool, include_audio: bool) -> str:
        """Generate a compact summary format."""
        lines = []
        lines.append(f"Video Duration: {VideoSegment._format_time(self.video_duration_seconds)}")
        lines.append("")
        
        if include_visual and self.frame_analyses:
            lines.append("Visual Content:")
            # Group similar descriptions
            seen = set()
            for fa in self.frame_analyses:
                if fa.description not in seen:
                    seen.add(fa.description)
                    lines.append(f"  - {fa.description}")
            lines.append("")
        
        if include_audio and self.full_transcript:
            lines.append("Audio Transcript:")
            lines.append(self.full_transcript)
        
        return "\n".join(lines).strip()


class VideoAnalyzer:
    """
    Lightweight video analysis orchestrator.
    
    Extracts frames and audio from videos locally (FFmpeg + OpenCV),
    then delegates visual analysis to the vision-service and audio
    transcription to the transcription-service via HTTP.
    """
    
    def __init__(
        self,
        vision_service_url: str = "http://vision-service:8004",
        transcription_service_url: str = "http://transcription-service:8005",
        request_timeout_seconds: int = 120,
        temp_dir: str = "/tmp/video_processing"
    ):
        self.vision_service_url = vision_service_url.rstrip("/")
        self.transcription_service_url = transcription_service_url.rstrip("/")
        self.request_timeout = request_timeout_seconds
        self.temp_dir = temp_dir
        os.makedirs(temp_dir, exist_ok=True)
        
        # Create a reusable async HTTP client
        self._client: Optional[httpx.AsyncClient] = None
        
        logger.info(
            f"VideoAnalyzer initialized — vision: {self.vision_service_url}, "
            f"transcription: {self.transcription_service_url}"
        )
    
    async def _get_client(self) -> httpx.AsyncClient:
        """Get or create the shared httpx async client."""
        if self._client is None or self._client.is_closed:
            self._client = httpx.AsyncClient(
                timeout=httpx.Timeout(self.request_timeout, connect=10.0)
            )
        return self._client
    
    async def close(self):
        """Close the HTTP client."""
        if self._client and not self._client.is_closed:
            await self._client.aclose()
    
    # ── upstream service calls ──────────────────────────────────────
    
    async def _analyze_frame_via_service(self, frame_path: str) -> str:
        """Send a frame image to the vision-service and return the description."""
        client = await self._get_client()
        
        with open(frame_path, "rb") as f:
            files = {"file": ("frame.jpg", f, "image/jpeg")}
            response = await client.post(
                f"{self.vision_service_url}/vision/analyze",
                files=files
            )
        
        response.raise_for_status()
        data = response.json()
        # Use detailed_caption if available, otherwise fall back to description
        return data.get("detailed_caption") or data.get("description", "")
    
    async def _transcribe_audio_via_service(
        self,
        audio_path: str,
        language: Optional[str] = None
    ) -> tuple:
        """
        Send an audio file to the transcription-service.
        Returns (full_text, segments) tuple.
        """
        client = await self._get_client()
        
        with open(audio_path, "rb") as f:
            files = {"file": ("audio.wav", f, "audio/wav")}
            data = {
                "task": "transcribe",
                "include_timestamps": "true"
            }
            if language:
                data["language"] = language
            
            response = await client.post(
                f"{self.transcription_service_url}/transcribe/file",
                files=files,
                data=data
            )
        
        response.raise_for_status()
        result = response.json()
        
        full_text = result.get("text", "")
        segments = []
        for seg in result.get("segments", []):
            start = seg.get("start") or seg.get("start_time")
            end = seg.get("end") or seg.get("end_time")
            text = seg.get("text", "").strip()
            if start is not None and end is not None and text:
                segments.append(TranscriptionSegment(
                    text=text,
                    start_time=float(start),
                    end_time=float(end)
                ))
        
        return full_text, segments
    
    # ── local extraction (FFmpeg / OpenCV) ──────────────────────────
    
    def get_video_info(self, video_path: str) -> Dict[str, Any]:
        """Get video metadata using FFprobe."""
        cmd = [
            "ffprobe",
            "-v", "quiet",
            "-print_format", "json",
            "-show_format",
            "-show_streams",
            video_path
        ]
        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0:
            raise ValueError(f"Failed to get video info: {result.stderr}")
        
        info = json.loads(result.stdout)
        
        video_stream = next(
            (s for s in info.get("streams", []) if s["codec_type"] == "video"), None
        )
        audio_stream = next(
            (s for s in info.get("streams", []) if s["codec_type"] == "audio"), None
        )
        
        duration = float(info.get("format", {}).get("duration", 0))
        width = int(video_stream.get("width", 0)) if video_stream else 0
        height = int(video_stream.get("height", 0)) if video_stream else 0
        
        fps = 30.0
        if video_stream and "r_frame_rate" in video_stream:
            fps_parts = video_stream["r_frame_rate"].split("/")
            if len(fps_parts) == 2 and int(fps_parts[1]) != 0:
                fps = float(fps_parts[0]) / float(fps_parts[1])
        
        return {
            "duration": duration,
            "width": width,
            "height": height,
            "fps": fps,
            "has_audio": audio_stream is not None
        }
    
    def extract_audio(self, video_path: str, output_path: str) -> bool:
        """Extract audio from video using FFmpeg."""
        cmd = [
            "ffmpeg",
            "-i", video_path,
            "-vn",
            "-acodec", "pcm_s16le",
            "-ar", "16000",
            "-ac", "1",
            "-y",
            output_path
        ]
        result = subprocess.run(cmd, capture_output=True, text=True)
        return result.returncode == 0
    
    def extract_frames(
        self,
        video_path: str,
        interval_seconds: float = 5.0,
        max_frames: int = 100
    ) -> List[tuple]:
        """
        Extract frames from video at regular intervals using OpenCV.
        Returns list of (timestamp, frame_path) tuples.
        """
        video_info = self.get_video_info(video_path)
        duration = video_info["duration"]
        
        timestamps = []
        current_time = 0.0
        while current_time < duration and len(timestamps) < max_frames:
            timestamps.append(current_time)
            current_time += interval_seconds
        
        cap = cv2.VideoCapture(video_path)
        fps = cap.get(cv2.CAP_PROP_FPS)
        
        frames = []
        for i, timestamp in enumerate(timestamps):
            frame_number = int(timestamp * fps)
            cap.set(cv2.CAP_PROP_POS_FRAMES, frame_number)
            ret, frame = cap.read()
            if ret:
                frame_path = os.path.join(self.temp_dir, f"frame_{i:04d}.jpg")
                cv2.imwrite(frame_path, frame)
                frames.append((timestamp, frame_path))
        
        cap.release()
        return frames
    
    # ── main pipeline ───────────────────────────────────────────────
    
    async def analyze_video(
        self,
        video_path: str,
        frame_interval_seconds: float = 5.0,
        max_frames: int = 100,
        transcribe: bool = True,
        analyze_visuals: bool = True,
        language: Optional[str] = None
    ) -> VideoAnalysisResult:
        """
        Analyze a video by extracting frames and audio locally,
        then delegating analysis to the vision and transcription services.
        """
        start_time = time.time()
        
        video_info = self.get_video_info(video_path)
        duration = video_info["duration"]
        has_audio = video_info["has_audio"]
        
        logger.info(
            f"Analyzing video: {duration:.1f}s, "
            f"{video_info['width']}x{video_info['height']}, "
            f"has_audio={has_audio}"
        )
        
        frame_analyses: List[FrameAnalysis] = []
        full_transcript = ""
        transcript_segments: List[TranscriptionSegment] = []
        
        # ── extract frames and audio in parallel (I/O-bound) ──
        frames = []
        audio_path = os.path.join(self.temp_dir, "temp_audio.wav")
        audio_extracted = False
        
        if analyze_visuals:
            logger.info(f"Extracting frames every {frame_interval_seconds}s")
            frames = self.extract_frames(video_path, frame_interval_seconds, max_frames)
        
        if transcribe and has_audio:
            logger.info("Extracting audio track")
            audio_extracted = self.extract_audio(video_path, audio_path)
        
        # ── call upstream services concurrently ──
        tasks = []
        
        # Queue frame analysis tasks
        if analyze_visuals and frames:
            logger.info(f"Sending {len(frames)} frames to vision-service")
            for i, (timestamp, frame_path) in enumerate(frames):
                tasks.append(self._analyze_single_frame(i, timestamp, frame_path))
        
        # Queue transcription task
        transcription_task = None
        if transcribe and has_audio and audio_extracted:
            logger.info("Sending audio to transcription-service")
            transcription_task = asyncio.create_task(
                self._transcribe_audio_via_service(audio_path, language)
            )
        
        # Await all frame analyses concurrently
        if tasks:
            frame_results = await asyncio.gather(*tasks, return_exceptions=True)
            for result in frame_results:
                if isinstance(result, FrameAnalysis):
                    frame_analyses.append(result)
                elif isinstance(result, Exception):
                    logger.warning(f"Frame analysis failed: {result}")
            # Sort by timestamp
            frame_analyses.sort(key=lambda fa: fa.timestamp_seconds)
        
        # Await transcription
        if transcription_task:
            try:
                full_transcript, transcript_segments = await transcription_task
            except Exception as e:
                logger.warning(f"Transcription failed: {e}")
        
        # ── cleanup temp files ──
        for _, frame_path in frames:
            if os.path.exists(frame_path):
                os.remove(frame_path)
        if os.path.exists(audio_path):
            os.remove(audio_path)
        
        # ── combine into unified segments ──
        segments = self._create_unified_segments(
            duration, frame_analyses, transcript_segments, frame_interval_seconds
        )
        
        processing_time = (time.time() - start_time) * 1000
        
        return VideoAnalysisResult(
            segments=segments,
            full_transcript=full_transcript,
            frame_analyses=frame_analyses,
            video_duration_seconds=duration,
            processing_time_ms=processing_time,
            video_dimensions=(video_info["width"], video_info["height"]),
            fps=video_info["fps"],
            has_audio=has_audio,
            frames_analyzed=len(frame_analyses)
        )
    
    async def _analyze_single_frame(
        self, index: int, timestamp: float, frame_path: str
    ) -> FrameAnalysis:
        """Analyze one frame via the vision-service, returning a FrameAnalysis."""
        description = await self._analyze_frame_via_service(frame_path)
        return FrameAnalysis(
            timestamp_seconds=timestamp,
            description=description,
            frame_number=index
        )
    
    @staticmethod
    def _text_similarity(a: str, b: str) -> float:
        """Compute word-level Jaccard similarity between two strings."""
        if not a or not b:
            return 0.0
        words_a = set(a.lower().split())
        words_b = set(b.lower().split())
        if not words_a or not words_b:
            return 0.0
        intersection = words_a & words_b
        union = words_a | words_b
        return len(intersection) / len(union)

    def _create_unified_segments(
        self,
        duration: float,
        frame_analyses: List[FrameAnalysis],
        transcript_segments: List[TranscriptionSegment],
        interval: float,
        similarity_threshold: float = 0.65
    ) -> List[VideoSegment]:
        """
        Create unified video segments combining visual and audio data.
        Merges consecutive segments whose visual descriptions are similar
        (Jaccard similarity >= threshold) to avoid redundant chunks.
        """
        raw_segments: List[VideoSegment] = []
        current_time = 0.0
        
        while current_time < duration:
            end_time = min(current_time + interval, duration)
            
            visual_desc = None
            for fa in frame_analyses:
                if current_time <= fa.timestamp_seconds < end_time:
                    visual_desc = fa.description
                    break
            
            transcript_parts = []
            for ts in transcript_segments:
                if ts.end_time > current_time and ts.start_time < end_time:
                    transcript_parts.append(ts.text)
            
            transcript = " ".join(transcript_parts).strip() if transcript_parts else None
            
            if visual_desc or transcript:
                raw_segments.append(VideoSegment(
                    start_time=current_time,
                    end_time=end_time,
                    visual_description=visual_desc,
                    transcript=transcript
                ))
            
            current_time = end_time
        
        # ── merge consecutive segments with similar visuals ──
        if not raw_segments:
            return raw_segments
        
        merged: List[VideoSegment] = [raw_segments[0]]
        
        for seg in raw_segments[1:]:
            prev = merged[-1]
            
            # Check if visual descriptions are similar
            visuals_similar = (
                prev.visual_description
                and seg.visual_description
                and self._text_similarity(prev.visual_description, seg.visual_description)
                    >= similarity_threshold
            )
            
            if visuals_similar:
                # Extend the previous segment's time range
                prev.end_time = seg.end_time
                # Append any new transcript text
                if seg.transcript:
                    if prev.transcript:
                        prev.transcript = f"{prev.transcript} {seg.transcript}"
                    else:
                        prev.transcript = seg.transcript
            else:
                merged.append(seg)
        
        logger.info(
            f"Segments: {len(raw_segments)} raw -> {len(merged)} after dedup "
            f"(threshold={similarity_threshold})"
        )
        
        return merged
    
    async def analyze_video_async(self, video_path: str, **kwargs) -> VideoAnalysisResult:
        """Async entry point — analyze_video is already async now."""
        return await self.analyze_video(video_path, **kwargs)

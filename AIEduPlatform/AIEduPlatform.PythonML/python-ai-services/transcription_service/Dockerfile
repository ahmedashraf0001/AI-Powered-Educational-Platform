FROM python:3.11-slim

ENV PYTHONUNBUFFERED=1

WORKDIR /app

# Install system dependencies for audio processing
RUN apt-get update && apt-get install -y \
    curl \
    git \
    ffmpeg \
    libsndfile1 \
    espeak-ng \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install -r requirements.txt

# Set cache directories BEFORE downloading so models land in the right place
ENV HF_HOME=/root/.cache/huggingface
ENV TTS_HOME=/root/.local/share/tts

# Pre-download Whisper model — NO cache mount so it stays in the image
RUN python -c "from transformers import AutoModelForSpeechSeq2Seq, AutoProcessor; \
    AutoProcessor.from_pretrained('openai/whisper-small'); \
    AutoModelForSpeechSeq2Seq.from_pretrained('openai/whisper-small')"

# Accept Coqui TOS non-interactively (required for XTTS v2 CPML license)
ENV COQUI_TOS_AGREED=1

# Pre-download TTS model (XTTS v2 — natural, human-like voices)
# ~1.8 GB model with breathing, pauses, and natural prosody
RUN python -c "from TTS.api import TTS; TTS(model_name='tts_models/multilingual/multi-dataset/xtts_v2')"

# Copy application code
COPY ./app ./app

# Expose port
EXPOSE 8005

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8005"]

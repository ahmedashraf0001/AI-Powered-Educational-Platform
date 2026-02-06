# syntax=docker/dockerfile:1.4
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies for audio processing
RUN apt-get update && apt-get install -y \
    curl \
    git \
    ffmpeg \
    libsndfile1 \
    espeak-ng \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies (cache mount speeds up rebuilds)
COPY requirements.txt .
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install -r requirements.txt

# Set cache directories BEFORE downloading so models land in the right place
ENV HF_HOME=/root/.cache/huggingface
ENV TTS_HOME=/root/.local/share/tts

# Pre-download Whisper model — NO cache mount so it stays in the image
RUN python -c "from transformers import AutoModelForSpeechSeq2Seq, AutoProcessor; \
    AutoProcessor.from_pretrained('openai/whisper-small'); \
    AutoModelForSpeechSeq2Seq.from_pretrained('openai/whisper-small')"

# Pre-download TTS model — NO cache mount so it stays in the image
RUN python -c "from TTS.api import TTS; TTS(model_name='tts_models/en/vctk/vits')"

# Copy application code
COPY ./app ./app

# Expose port
EXPOSE 8005

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8005"]

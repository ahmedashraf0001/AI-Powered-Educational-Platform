FROM python:3.11-slim

WORKDIR /app

ENV PYTHONUNBUFFERED=1

# Install system dependencies (FFmpeg for audio/video extraction, libGL for OpenCV)
RUN apt-get update && apt-get install -y \
    curl \
    ffmpeg \
    libsm6 \
    libxext6 \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install -r requirements.txt

# Copy application code
COPY ./app ./app

# Create temp directory for video processing
RUN mkdir -p /tmp/video_processing

# Expose port
EXPOSE 8006

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8006"]

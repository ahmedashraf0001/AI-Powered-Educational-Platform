# syntax=docker/dockerfile:1.4
FROM python:3.11-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies with cache mount
COPY requirements.txt .
RUN --mount=type=cache,target=/root/.cache/pip \
    pip install -r requirements.txt

# Pre-download BLIP model for image captioning (cached via HuggingFace cache)
RUN --mount=type=cache,target=/root/.cache/huggingface \
    python -c "from transformers import BlipProcessor, BlipForConditionalGeneration; \
    BlipProcessor.from_pretrained('Salesforce/blip-image-captioning-large'); \
    BlipForConditionalGeneration.from_pretrained('Salesforce/blip-image-captioning-large')" || true

# Set HuggingFace cache directory
ENV HF_HOME=/root/.cache/huggingface
ENV TRANSFORMERS_CACHE=/root/.cache/huggingface

# Copy application code
COPY ./app ./app

# Expose port
EXPOSE 8004

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8004"]

FROM python:3.11-slim

WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements and install Python dependencies
COPY requirements.txt .
RUN pip install -r requirements.txt

# Set HuggingFace cache directory BEFORE downloading so model lands in the right place
ENV HF_HOME=/root/.cache/huggingface
ENV PYTHONUNBUFFERED=1

# Pre-download BLIP model — NO cache mount so it stays in the image
RUN python -c "from transformers import BlipProcessor, BlipForConditionalGeneration; \
    BlipProcessor.from_pretrained('Salesforce/blip-image-captioning-large'); \
    BlipForConditionalGeneration.from_pretrained('Salesforce/blip-image-captioning-large')"

# Copy application code
COPY ./app ./app

# Expose port
EXPOSE 8004

# Run the application
CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8004"]

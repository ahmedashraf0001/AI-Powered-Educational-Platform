# ML Services

High-performance microservices for embeddings and reranking using FastAPI and sentence-transformers.

## Services

### Embedding Service (Port 8000)
- Single text embedding: `POST /api/embeddings/single`
- Batch embeddings: `POST /api/embeddings/batch`
- Health check: `GET /health`

### Reranking Service (Port 8001)
- Score pairs: `POST /api/rerank/score-pairs`
- Rerank passages: `POST /api/rerank/rerank`
- Health check: `GET /health`

## Quick Start

### Using Docker Compose (Recommended)
```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

### Manual Setup

#### Embedding Service
```bash
cd embedding_service
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8000
```

#### Reranking Service
```bash
cd reranking_service
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
uvicorn app.main:app --host 0.0.0.0 --port 8001
```

## API Examples

### Embedding Service

**Single Embedding:**
```bash
curl -X POST "http://localhost:8000/api/embeddings/single" \
  -H "Content-Type: application/json" \
  -d '{"text": "This is a test sentence"}'
```

**Batch Embeddings:**
```bash
curl -X POST "http://localhost:8000/api/embeddings/batch" \
  -H "Content-Type: application/json" \
  -d '{"texts": ["First sentence", "Second sentence"]}'
```

### Reranking Service

**Rerank Passages:**
```bash
curl -X POST "http://localhost:8001/api/rerank/rerank" \
  -H "Content-Type: application/json" \
  -d '{
    "query": "What is machine learning?",
    "passages": [
      "Machine learning is a subset of AI",
      "The weather is sunny today",
      "Deep learning uses neural networks"
    ],
    "top_k": 2
  }'
```

## Configuration

Both services support environment variables (see `.env` files):

- `MODEL_NAME`: HuggingFace model identifier
- `DEVICE`: `cpu` or `cuda`
- `MAX_BATCH_SIZE`: Maximum batch size for processing
- `MODEL_CACHE_DIR`: Directory to cache downloaded models

## Interactive Documentation

- Embedding Service: http://localhost:8000/docs
- Reranking Service: http://localhost:8001/docs

## License

MIT
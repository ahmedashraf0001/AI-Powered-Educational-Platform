from pydantic import BaseModel, Field, validator
from typing import List, Optional, Dict, Any

class EmbeddingRequest(BaseModel):
    text: str = Field(
        ..., 
        min_length=1,
        max_length=8192,
        description="Text to generate embedding for"
    )
    normalize: bool = Field(
        default=True,
        description="Whether to normalize the embedding vector"
    )
    
    @validator('text')
    def text_not_empty(cls, v):
        if not v.strip():
            raise ValueError('Text cannot be empty or whitespace only')
        return v

class BatchEmbeddingRequest(BaseModel):
    texts: List[str] = Field(
        ...,
        min_items=1,
        max_items=100,
        description="List of texts to generate embeddings for"
    )
    normalize: bool = Field(
        default=True,
        description="Whether to normalize the embedding vectors"
    )
    batch_size: Optional[int] = Field(
        default=None,
        ge=1,
        le=32,
        description="Batch size for processing (optional)"
    )
    
    @validator('texts')
    def validate_texts(cls, v):
        for idx, text in enumerate(v):
            if not text or not text.strip():
                raise ValueError(f'Text at index {idx} is empty')
        return v

class EmbeddingResponse(BaseModel):
    embedding: List[float]
    dimension: int
    model: str

class BatchEmbeddingResponse(BaseModel):
    embeddings: List[List[float]]
    count: int
    dimension: int
    model: str


class ChunkResult(BaseModel):
    """Result for a single chunk in batch processing."""
    index: int = Field(..., description="Index of the chunk in the input list")
    success: bool = Field(..., description="Whether embedding generation succeeded")
    embedding: Optional[List[float]] = Field(None, description="Embedding vector if successful")
    error: Optional[str] = Field(None, description="Error message if failed")
    text_length: Optional[int] = Field(None, description="Length of the input text")
    was_truncated: bool = Field(False, description="Whether text was truncated")


class EmbeddingChunk(BaseModel):
    """Single chunk with index for batch processing."""
    index: int = Field(..., description="Index/identifier for this chunk")
    text: str = Field(..., description="Text to generate embedding for")


class DetailedBatchRequest(BaseModel):
    """Request for batch embedding with detailed error reporting."""
    texts: List[EmbeddingChunk] = Field(
        ...,
        min_items=1,
        max_items=100,
        description="List of text chunks with indices to generate embeddings for"
    )
    normalize: bool = Field(
        default=True,
        description="Whether to normalize the embedding vectors"
    )
    continue_on_error: bool = Field(
        default=False,
        description="Continue processing remaining chunks if one fails"
    )


class DetailedBatchResponse(BaseModel):
    """Response with per-chunk results and error reporting."""
    results: List[ChunkResult] = Field(..., description="Results for each chunk")
    total_chunks: int = Field(..., description="Total number of input chunks")
    successful: int = Field(..., description="Number of successfully processed chunks")
    failed: int = Field(..., description="Number of failed chunks")
    dimension: int = Field(..., description="Embedding dimension")
    model: str = Field(..., description="Model used for embeddings")
    errors_summary: Optional[List[Dict[str, Any]]] = Field(
        None,
        description="Summary of all errors with chunk indices"
    )

class HealthResponse(BaseModel):
    status: str
    model: str
    dimension: int
    device: str
    
class DetailedHealthResponse(BaseModel):
    status: str
    model: Dict[str, Any]
    system: Dict[str, Any]
    gpu: Optional[Dict[str, Any]] = None
    config: Dict[str, Any]
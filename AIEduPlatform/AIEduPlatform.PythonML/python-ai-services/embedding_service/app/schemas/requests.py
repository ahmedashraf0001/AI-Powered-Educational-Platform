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
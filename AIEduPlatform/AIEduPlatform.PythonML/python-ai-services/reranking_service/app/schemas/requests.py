from pydantic import BaseModel, Field, validator
from typing import List, Optional, Dict, Any

class QueryPassagePair(BaseModel):
    query: str = Field(..., min_length=1, max_length=4096)
    passage: str = Field(..., min_length=1, max_length=4096)
    
    @validator('query', 'passage')
    def text_not_empty(cls, v):
        if not v.strip():
            raise ValueError('Text cannot be empty or whitespace only')
        return v


class RerankChunk(BaseModel):
    """Single chunk with index for reranking."""
    index: int = Field(..., description="Index/identifier for this chunk")
    content: str = Field(..., min_length=1, description="Text content of the chunk")
    
    @validator('content')
    def content_not_empty(cls, v):
        if not v.strip():
            raise ValueError('Content cannot be empty or whitespace only')
        return v

class ScorePairsRequest(BaseModel):
    pairs: List[QueryPassagePair] = Field(
        ...,
        min_items=1,
        max_items=100,
        description="List of query-passage pairs to score"
    )
    batch_size: Optional[int] = Field(
        default=None,
        ge=1,
        le=32,
        description="Batch size for processing (optional)"
    )

class RerankRequest(BaseModel):
    query: str = Field(
        ...,
        min_length=1,
        max_length=4096,
        description="Search query"
    )
    chunks: List[RerankChunk] = Field(
        ...,
        min_items=1,
        max_items=100,
        description="List of chunks with index and content to rerank"
    )
    top_k: Optional[int] = Field(
        default=None,
        ge=1,
        le=100,
        description="Number of top results to return"
    )
    return_content: bool = Field(
        default=True,
        description="Whether to include chunk content in response"
    )
    
    @validator('query')
    def query_not_empty(cls, v):
        if not v.strip():
            raise ValueError('Query cannot be empty or whitespace only')
        return v

class ScoreResult(BaseModel):
    index: int
    score: float

class RerankResult(BaseModel):
    index: int = Field(..., description="Original index of the chunk")
    score: float = Field(..., description="Relevance score")
    content: Optional[str] = Field(None, description="Chunk content if requested")

class ScorePairsResponse(BaseModel):
    scores: List[float]
    count: int
    model: str

class RerankResponse(BaseModel):
    results: List[RerankResult]
    count: int
    model: str

class HealthResponse(BaseModel):
    status: str
    model: str
    device: str

class DetailedHealthResponse(BaseModel):
    status: str
    model: Dict[str, Any]
    system: Dict[str, Any]
    gpu: Optional[Dict[str, Any]] = None
    config: Dict[str, Any]
from fastapi import APIRouter, HTTPException, status
from app.schemas.requests import (
    EmbeddingRequest, 
    BatchEmbeddingRequest,
    EmbeddingResponse,
    BatchEmbeddingResponse,
    DetailedBatchRequest,
    DetailedBatchResponse,
    ChunkResult
)
from app.models.embedder import embedding_model
from app.config import get_settings
import asyncio
import logging

logger = logging.getLogger(__name__)
router = APIRouter()
settings = get_settings()

@router.post("/single", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    """Generate embedding for a single text"""
    try:
        embedding = await asyncio.to_thread(
            embedding_model.encode_single,
            request.text,
            normalize=request.normalize
        )
        
        return EmbeddingResponse(
            embedding=embedding,
            dimension=len(embedding),
            model=settings.MODEL_NAME
        )
    except ValueError as e:
        logger.warning(f"Validation error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error generating embedding: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to generate embedding"
        )

@router.post("/batch", response_model=BatchEmbeddingResponse)
async def create_batch_embeddings(request: BatchEmbeddingRequest):
    """Generate embeddings for multiple texts"""
    try:
        embeddings = await asyncio.to_thread(
            embedding_model.encode_batch,
            request.texts,
            normalize=request.normalize,
            batch_size=request.batch_size
        )
        
        return BatchEmbeddingResponse(
            embeddings=embeddings,
            count=len(embeddings),
            dimension=len(embeddings[0]) if embeddings else 0,
            model=settings.MODEL_NAME
        )
    except ValueError as e:
        logger.warning(f"Validation error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error generating batch embeddings: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to generate embeddings"
        )


@router.post("/batch/detailed", response_model=DetailedBatchResponse)
async def create_batch_embeddings_detailed(request: DetailedBatchRequest):
    """
    Generate embeddings for multiple texts with detailed per-chunk error reporting.
    
    Unlike `/batch`, this endpoint:
    - Processes each chunk individually with user-provided indices
    - Reports success/failure for each chunk with its index
    - Continues processing even if some chunks fail (configurable)
    - Provides error messages and metadata for each chunk
    
    Use this when you need to know exactly which chunks failed and why.
    
    Request format:
    ```json
    {
        "texts": [
            {"index": 0, "text": "First chunk"},
            {"index": 1, "text": "Second chunk"}
        ],
        "normalize": true,
        "continue_on_error": false
    }
    ```
    """
    try:
        # Convert EmbeddingChunk objects to dicts
        chunks = [{"index": chunk.index, "text": chunk.text} for chunk in request.texts]
        
        result = await asyncio.to_thread(
            embedding_model.encode_batch_detailed,
            chunks,
            normalize=request.normalize,
            continue_on_error=request.continue_on_error
        )
        
        # Convert dict results to ChunkResult models
        chunk_results = [
            ChunkResult(
                index=r["index"],
                success=r["success"],
                embedding=r["embedding"],
                error=r["error"],
                text_length=r["text_length"],
                was_truncated=r["was_truncated"]
            )
            for r in result["results"]
        ]
        
        return DetailedBatchResponse(
            results=chunk_results,
            total_chunks=result["total_chunks"],
            successful=result["successful"],
            failed=result["failed"],
            dimension=result["dimension"],
            model=settings.MODEL_NAME,
            errors_summary=result["errors_summary"]
        )
        
    except ValueError as e:
        logger.warning(f"Validation error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error in detailed batch processing: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to process batch: {str(e)}"
        )
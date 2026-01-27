from fastapi import APIRouter, HTTPException, status
from app.schemas.requests import (
    EmbeddingRequest, 
    BatchEmbeddingRequest,
    EmbeddingResponse,
    BatchEmbeddingResponse
)
from app.models.embedder import embedding_model
from app.config import get_settings
import logging

logger = logging.getLogger(__name__)
router = APIRouter()
settings = get_settings()

@router.post("/single", response_model=EmbeddingResponse)
async def create_embedding(request: EmbeddingRequest):
    """Generate embedding for a single text"""
    try:
        embedding = embedding_model.encode_single(
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
        embeddings = embedding_model.encode_batch(
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
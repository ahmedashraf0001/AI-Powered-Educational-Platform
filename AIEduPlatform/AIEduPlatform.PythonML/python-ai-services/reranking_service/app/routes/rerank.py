from fastapi import APIRouter, HTTPException, status
from app.schemas.requests import (
    ScorePairsRequest,
    RerankRequest,
    ScorePairsResponse,
    RerankResponse
)
from app.models.reranker import reranking_model
from app.config import get_settings
import logging

logger = logging.getLogger(__name__)
router = APIRouter()
settings = get_settings()

@router.post("/score-pairs", response_model=ScorePairsResponse)
async def score_pairs(request: ScorePairsRequest):
    """Score query-passage pairs"""
    try:
        pairs = [(p.query, p.passage) for p in request.pairs]
        
        scores = reranking_model.predict_scores(
            pairs,
            batch_size=request.batch_size
        )
        
        return ScorePairsResponse(
            scores=scores,
            count=len(scores),
            model=settings.MODEL_NAME
        )
    except ValueError as e:
        logger.warning(f"Validation error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error scoring pairs: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to score pairs"
        )

@router.post("/rerank", response_model=RerankResponse)
async def rerank_passages(request: RerankRequest):
    """Rerank chunks based on query relevance
    
    Request format:
    ```json
    {
        "query": "search query",
        "chunks": [
            {"index": 0, "content": "First chunk text"},
            {"index": 5, "content": "Another chunk"},
            {"index": 12, "content": "Third chunk"}
        ],
        "top_k": 10,
        "return_content": true
    }
    ```
    
    Response preserves original indices for easy mapping.
    """
    try:
        # Convert RerankChunk objects to dicts
        chunks = [{"index": c.index, "content": c.content} for c in request.chunks]
        
        results = reranking_model.rerank(
            query=request.query,
            chunks=chunks,
            top_k=request.top_k,
            return_content=request.return_content
        )
        
        return RerankResponse(
            results=results,
            count=len(results),
            model=settings.MODEL_NAME
        )
    except ValueError as e:
        logger.warning(f"Validation error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        logger.error(f"Error reranking passages: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to rerank passages"
        )
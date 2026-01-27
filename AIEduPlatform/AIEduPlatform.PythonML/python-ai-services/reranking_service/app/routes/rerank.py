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
    """Rerank passages based on query relevance"""
    try:
        results = reranking_model.rerank(
            query=request.query,
            passages=request.passages,
            top_k=request.top_k,
            return_documents=request.return_documents
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
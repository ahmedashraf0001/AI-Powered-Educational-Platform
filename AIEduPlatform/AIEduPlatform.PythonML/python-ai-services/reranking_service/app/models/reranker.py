from sentence_transformers import CrossEncoder
from typing import List, Tuple, Dict
import numpy as np
import torch
import logging
from app.config import get_settings

logger = logging.getLogger(__name__)

class RerankingModel:
    """Singleton wrapper for the reranking model"""
    _instance = None
    _model = None
    
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super().__new__(cls)
        return cls._instance
    
    def __init__(self):
        if self._model is None:
            self._load_model()
    
    def _load_model(self):
        """Load the reranking model on first use"""
        settings = get_settings()
        try:
            logger.info(f"Loading reranking model: {settings.MODEL_NAME}")
            
            self._model = CrossEncoder(
                settings.MODEL_NAME,
                max_length=settings.MAX_TEXT_LENGTH,
                device=settings.DEVICE
            )
            
            logger.info("Reranking model loaded successfully")
        except Exception as e:
            logger.error(f"Failed to load model: {str(e)}")
            raise
    
    def predict_scores(
        self, 
        query_passage_pairs: List[Tuple[str, str]],
        batch_size: int = None
    ) -> List[float]:
        """
        Predict relevance scores for query-passage pairs
        
        Args:
            query_passage_pairs: List of (query, passage) tuples
            batch_size: Batch size for processing
            
        Returns:
            List of relevance scores
        """
        if not query_passage_pairs:
            raise ValueError("Pairs list cannot be empty")
        
        settings = get_settings()
        
        if len(query_passage_pairs) > settings.MAX_PAIRS:
            raise ValueError(
                f"Too many pairs. Maximum allowed: {settings.MAX_PAIRS}"
            )
        
        # Validate and truncate texts
        processed_pairs = []
        for idx, (query, passage) in enumerate(query_passage_pairs):
            if not query or not query.strip():
                raise ValueError(f"Query at index {idx} is empty")
            if not passage or not passage.strip():
                raise ValueError(f"Passage at index {idx} is empty")
            
            # Truncate if needed
            if len(query) > settings.MAX_TEXT_LENGTH:
                logger.warning(
                    f"Query at index {idx} truncated from {len(query)} "
                    f"to {settings.MAX_TEXT_LENGTH} characters"
                )
                query = query[:settings.MAX_TEXT_LENGTH]
            
            if len(passage) > settings.MAX_TEXT_LENGTH:
                logger.warning(
                    f"Passage at index {idx} truncated from {len(passage)} "
                    f"to {settings.MAX_TEXT_LENGTH} characters"
                )
                passage = passage[:settings.MAX_TEXT_LENGTH]
            
            processed_pairs.append([query, passage])
        
        # Use configured batch size if not specified
        if batch_size is None:
            batch_size = min(settings.MAX_BATCH_SIZE, len(processed_pairs))
        
        with torch.no_grad():
            scores = self._model.predict(
                processed_pairs,
                batch_size=batch_size,
                show_progress_bar=False
            )
        
        return scores.tolist()
    
    def rerank(
        self,
        query: str,
        passages: List[str],
        top_k: int = None,
        return_documents: bool = True
    ) -> List[Dict]:
        """
        Rerank passages based on relevance to query
        
        Args:
            query: Search query
            passages: List of passage texts
            top_k: Number of top results to return (None = all)
            return_documents: Whether to include document text in results
            
        Returns:
            List of dicts with index, score, and optionally document
        """
        if not query or not query.strip():
            raise ValueError("Query cannot be empty")
        
        if not passages:
            raise ValueError("Passages list cannot be empty")
        
        # Create pairs
        pairs = [(query, passage) for passage in passages]
        
        # Get scores
        scores = self.predict_scores(pairs)
        
        # Create results with indices
        results = [
            {
                "index": idx,
                "score": float(score),
                "document": passages[idx] if return_documents else None
            }
            for idx, score in enumerate(scores)
        ]
        
        # Sort by score (descending)
        results.sort(key=lambda x: x["score"], reverse=True)
        
        # Return top_k if specified
        if top_k is not None and top_k > 0:
            results = results[:top_k]
        
        # Remove document field if not requested
        if not return_documents:
            for r in results:
                del r["document"]
        
        return results

# Global instance
reranking_model = RerankingModel()
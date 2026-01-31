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
        chunks: List[Dict],
        top_k: int = None,
        return_content: bool = True
    ) -> List[Dict]:
        """
        Rerank chunks based on relevance to query
        
        Args:
            query: Search query
            chunks: List of dicts with 'index' and 'content' keys
            top_k: Number of top results to return (None = all)
            return_content: Whether to include content text in results
            
        Returns:
            List of dicts with index, score, and optionally content
        """
        if not query or not query.strip():
            raise ValueError("Query cannot be empty")
        
        if not chunks:
            raise ValueError("Chunks list cannot be empty")
        
        settings = get_settings()
        
        # Build pairs and track original indices
        pairs = []
        chunk_map = {}  # maps position in pairs list to original chunk
        
        for pos, chunk in enumerate(chunks):
            chunk_index = chunk.get("index", pos)
            chunk_content = chunk.get("content", "")
            
            if not chunk_content or not chunk_content.strip():
                logger.warning(f"Chunk at index {chunk_index} has empty content, skipping")
                continue
            
            # Truncate if needed
            if len(chunk_content) > settings.MAX_TEXT_LENGTH:
                logger.warning(
                    f"Chunk at index {chunk_index} truncated from {len(chunk_content)} "
                    f"to {settings.MAX_TEXT_LENGTH} characters"
                )
                chunk_content = chunk_content[:settings.MAX_TEXT_LENGTH]
            
            pairs.append([query, chunk_content])
            chunk_map[len(pairs) - 1] = {
                "index": chunk_index,
                "content": chunk.get("content", "")
            }
        
        if not pairs:
            raise ValueError("No valid chunks to rerank")
        
        # Get scores
        with torch.no_grad():
            scores = self._model.predict(
                pairs,
                batch_size=min(settings.MAX_BATCH_SIZE, len(pairs)),
                show_progress_bar=False
            )
        
        # Create results with original indices
        results = []
        for pos, score in enumerate(scores):
            original = chunk_map[pos]
            result = {
                "index": original["index"],
                "score": float(score)
            }
            if return_content:
                result["content"] = original["content"]
            results.append(result)
        
        # Sort by score (descending)
        results.sort(key=lambda x: x["score"], reverse=True)
        
        # Return top_k if specified
        if top_k is not None and top_k > 0:
            results = results[:top_k]
        
        return results

# Global instance
reranking_model = RerankingModel()
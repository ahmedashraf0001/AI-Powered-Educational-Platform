from sentence_transformers import SentenceTransformer
from typing import List, Union
import numpy as np
import torch
import logging
from app.config import get_settings

logger = logging.getLogger(__name__)

class EmbeddingModel:
    """Singleton wrapper for the embedding model"""
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
        """Load the embedding model on first use"""
        settings = get_settings()
        try:
            logger.info(f"Loading embedding model: {settings.MODEL_NAME}")
            
            self._model = SentenceTransformer(
                settings.MODEL_NAME,
                cache_folder=settings.MODEL_CACHE_DIR,
                device=settings.DEVICE
            )
            
            # Set to evaluation mode
            self._model.eval()
            
            logger.info(
                f"Model loaded successfully. "
                f"Embedding dimension: {self._model.get_sentence_embedding_dimension()}"
            )
        except Exception as e:
            logger.error(f"Failed to load model: {str(e)}")
            raise
    
    def encode_single(self, text: str, normalize: bool = True) -> List[float]:
        """Generate embedding for a single text"""
        if not text or not text.strip():
            raise ValueError("Text cannot be empty")
        
        settings = get_settings()
        if len(text) > settings.MAX_TEXT_LENGTH:
            logger.warning(f"Text truncated from {len(text)} to {settings.MAX_TEXT_LENGTH} characters")
            text = text[:settings.MAX_TEXT_LENGTH]
        
        with torch.no_grad():
            embedding = self._model.encode(
                text,
                convert_to_numpy=True,
                normalize_embeddings=normalize,
                show_progress_bar=False
            )
        
        return embedding.tolist()
    
    def encode_batch(
        self, 
        texts: List[str], 
        normalize: bool = True,
        batch_size: int = None
    ) -> List[List[float]]:
        """Generate embeddings for multiple texts"""
        if not texts:
            raise ValueError("Text list cannot be empty")
        
        settings = get_settings()
        
        # Validate and truncate texts
        processed_texts = []
        for idx, text in enumerate(texts):
            if not text or not text.strip():
                raise ValueError(f"Text at index {idx} is empty")
            
            if len(text) > settings.MAX_TEXT_LENGTH:
                logger.warning(
                    f"Text at index {idx} truncated from {len(text)} "
                    f"to {settings.MAX_TEXT_LENGTH} characters"
                )
                text = text[:settings.MAX_TEXT_LENGTH]
            
            processed_texts.append(text)
        
        # Use configured batch size if not specified
        if batch_size is None:
            batch_size = min(settings.MAX_BATCH_SIZE, len(processed_texts))
        
        with torch.no_grad():
            embeddings = self._model.encode(
                processed_texts,
                convert_to_numpy=True,
                normalize_embeddings=normalize,
                batch_size=batch_size,
                show_progress_bar=False
            )
        
        return embeddings.tolist()
    
    @property
    def dimension(self) -> int:
        """Get the embedding dimension"""
        return self._model.get_sentence_embedding_dimension()
    
    @property
    def max_seq_length(self) -> int:
        """Get the maximum sequence length"""
        return self._model.max_seq_length

# Global instance
embedding_model = EmbeddingModel()
from typing import Optional, List
from PIL import Image
import torch
from transformers import BlipProcessor, BlipForConditionalGeneration
from dataclasses import dataclass
import io
import time


@dataclass
class VisionAnalysisResult:
    """Result from vision analysis."""
    description: str
    detailed_caption: str
    prompt_used: Optional[str]
    processing_time_ms: float
    image_dimensions: tuple
    model_name: str
    
    def to_dict(self) -> dict:
        return {
            "description": self.description,
            "detailed_caption": self.detailed_caption,
            "prompt_used": self.prompt_used,
            "processing_time_ms": self.processing_time_ms,
            "image_dimensions": {
                "width": self.image_dimensions[0],
                "height": self.image_dimensions[1]
            },
            "model_name": self.model_name
        }
    
    def to_llm_context(self, include_metadata: bool = False) -> str:
        """Format the result as context for an LLM."""
        if include_metadata:
            return f"[Image Analysis ({self.image_dimensions[0]}x{self.image_dimensions[1]})]\n{self.detailed_caption}"
        return self.detailed_caption


class VisionAnalyzer:
    """Vision analyzer using BLIP model for image captioning and description."""
    
    def __init__(
        self,
        model_name: str = "Salesforce/blip-image-captioning-large",
        use_gpu: bool = True
    ):
        self.model_name = model_name
        self.device = "cuda" if use_gpu and torch.cuda.is_available() else "cpu"
        self.torch_dtype = torch.float16 if self.device == "cuda" else torch.float32
        
        print(f"Loading vision model: {model_name} on {self.device} with dtype {self.torch_dtype}")
        self.processor = BlipProcessor.from_pretrained(model_name)
        self.model = BlipForConditionalGeneration.from_pretrained(
            model_name,
            torch_dtype=self.torch_dtype,
            low_cpu_mem_usage=True
        ).to(self.device)
        self.model.eval()
        
        # Enable memory efficient attention if available
        if self.device == "cuda" and hasattr(self.model, 'enable_attention_slicing'):
            self.model.enable_attention_slicing()
        
        print(f"Vision model loaded successfully (GPU: {self.device == 'cuda'})")
    
    def preprocess_image(
        self,
        image: Image.Image,
        max_size: int = 1024
    ) -> Image.Image:
        """Preprocess image for the model."""
        # Convert to RGB if necessary
        if image.mode != "RGB":
            image = image.convert("RGB")
        
        # Resize if too large
        if max(image.size) > max_size:
            ratio = max_size / max(image.size)
            new_size = (int(image.size[0] * ratio), int(image.size[1] * ratio))
            image = image.resize(new_size, Image.Resampling.LANCZOS)
        
        return image
    
    def generate_caption(
        self,
        image: Image.Image,
        max_new_tokens: int = 100,
        min_new_tokens: int = 10,
        num_beams: int = 4
    ) -> str:
        """Generate a basic caption for the image."""
        inputs = self.processor(image, return_tensors="pt").to(self.device)
        
        with torch.no_grad():
            output = self.model.generate(
                **inputs,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                num_beams=num_beams
            )
        
        caption = self.processor.decode(output[0], skip_special_tokens=True)
        return caption
    
    def generate_conditional_caption(
        self,
        image: Image.Image,
        prompt: str,
        max_new_tokens: int = 150,
        min_new_tokens: int = 20,
        num_beams: int = 4
    ) -> str:
        """Generate a caption conditioned on a prompt."""
        inputs = self.processor(image, prompt, return_tensors="pt").to(self.device)
        
        with torch.no_grad():
            output = self.model.generate(
                **inputs,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                num_beams=num_beams
            )
        
        caption = self.processor.decode(output[0], skip_special_tokens=True)
        return caption
    
    def analyze_image(
        self,
        image: Image.Image,
        prompt: Optional[str] = None,
        max_new_tokens: int = 200,
        min_new_tokens: int = 20,
        num_beams: int = 4,
        include_details: bool = True
    ) -> VisionAnalysisResult:
        """
        Analyze an image and generate detailed descriptions.
        
        Args:
            image: PIL Image to analyze
            prompt: Optional prompt to guide the description
            max_new_tokens: Maximum tokens in generated text
            min_new_tokens: Minimum tokens in generated text
            num_beams: Number of beams for beam search
            include_details: Whether to generate multiple descriptions
            
        Returns:
            VisionAnalysisResult with descriptions
        """
        start_time = time.time()
        original_size = image.size
        
        # Preprocess image
        processed = self.preprocess_image(image)
        
        # Generate basic caption
        basic_caption = self.generate_caption(
            processed,
            max_new_tokens=100,
            min_new_tokens=10,
            num_beams=num_beams
        )
        
        # Generate detailed description
        if include_details:
            detail_prompts = [
                "This image shows",
                "The image contains",
                "In detail, this picture depicts"
            ]
            
            descriptions = []
            for p in detail_prompts:
                desc = self.generate_conditional_caption(
                    processed,
                    p,
                    max_new_tokens=max_new_tokens,
                    min_new_tokens=min_new_tokens,
                    num_beams=num_beams
                )
                descriptions.append(desc)
            
            # Combine descriptions
            detailed_caption = f"{basic_caption}. " + " ".join(descriptions)
        else:
            detailed_caption = basic_caption
        
        # If custom prompt provided, also use it
        if prompt:
            custom_desc = self.generate_conditional_caption(
                processed,
                prompt,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                num_beams=num_beams
            )
            detailed_caption = f"{detailed_caption}\n\nBased on prompt '{prompt}': {custom_desc}"
        
        processing_time = (time.time() - start_time) * 1000
        
        return VisionAnalysisResult(
            description=basic_caption,
            detailed_caption=detailed_caption,
            prompt_used=prompt,
            processing_time_ms=processing_time,
            image_dimensions=original_size,
            model_name=self.model_name
        )
    
    def analyze_from_bytes(
        self,
        image_bytes: bytes,
        prompt: Optional[str] = None,
        max_new_tokens: int = 200,
        include_details: bool = True
    ) -> VisionAnalysisResult:
        """Analyze image from bytes."""
        image = Image.open(io.BytesIO(image_bytes))
        return self.analyze_image(
            image,
            prompt=prompt,
            max_new_tokens=max_new_tokens,
            include_details=include_details
        )
    
    def quick_describe(
        self,
        image: Image.Image,
        prompt: Optional[str] = None
    ) -> str:
        """Quick single description for fast inference."""
        processed = self.preprocess_image(image)
        
        if prompt:
            return self.generate_conditional_caption(processed, prompt, max_new_tokens=100)
        return self.generate_caption(processed, max_new_tokens=100)

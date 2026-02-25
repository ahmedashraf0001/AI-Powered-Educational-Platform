from typing import Optional
from PIL import Image
import torch
from transformers import Qwen2VLForConditionalGeneration, AutoProcessor
from dataclasses import dataclass
import io
import time
import shutil
from pathlib import Path
from qwen_vl_utils import process_vision_info
from tqdm.auto import tqdm
import sys


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
    """Vision analyzer using Qwen2-VL-2B model for image captioning and description."""
    
    def __init__(
        self,
        model_name: str = "Qwen/Qwen2-VL-2B-Instruct",
        use_gpu: bool = True
    ):
        self.model_name = model_name
        self.device = "cuda" if use_gpu and torch.cuda.is_available() else "cpu"
        
        # Qwen2-VL works best with bfloat16 on GPU, float32 on CPU
        if self.device == "cuda":
            self.torch_dtype = torch.bfloat16
        else:
            self.torch_dtype = torch.float32
        
        print(f"Loading vision model: {model_name} on {self.device} with dtype {self.torch_dtype}")
        
        # Progress callback for downloads
        def progress_callback(current, total):
            """Callback for download progress."""
            if total > 0:
                percent = (current / total) * 100
                mb_current = current / (1024 * 1024)
                mb_total = total / (1024 * 1024)
                print(f"Downloading: {mb_current:.1f}MB / {mb_total:.1f}MB ({percent:.1f}%)")
                sys.stdout.flush()
        
        # Try loading with cache first, force download if fails
        max_retries = 2
        for attempt in range(max_retries):
            try:
                print(f"Attempt {attempt + 1}/{max_retries} to load model...")
                sys.stdout.flush()
                
                # Load Qwen2-VL processor
                print("Loading processor...")
                sys.stdout.flush()
                self.processor = AutoProcessor.from_pretrained(
                    model_name,
                    force_download=(attempt > 0),
                    local_files_only=False,
                    trust_remote_code=True
                )
                print("✓ Processor loaded successfully")
                sys.stdout.flush()
                
                # Load Qwen2-VL model
                print(f"Loading model weights (this may take 5-10 minutes on first download)...")
                sys.stdout.flush()
                self.model = Qwen2VLForConditionalGeneration.from_pretrained(
                    model_name,
                    torch_dtype=self.torch_dtype,
                    force_download=(attempt > 0),
                    local_files_only=False,
                    trust_remote_code=True,
                    device_map="auto" if self.device == "cuda" else None
                )
                print("✓ Model weights loaded successfully")
                sys.stdout.flush()
                
                # Success - break the retry loop
                break
                
            except Exception as e:
                print(f"✗ Error on attempt {attempt + 1}: {e}")
                sys.stdout.flush()
                
                if attempt < max_retries - 1:
                    print("Retrying with force_download=True...")
                    sys.stdout.flush()
                    # Clear corrupted cache
                    cache_dir = Path.home() / ".cache" / "huggingface" / "hub"
                    if cache_dir.exists():
                        print(f"Clearing cache at {cache_dir}")
                        sys.stdout.flush()
                        for model_dir in cache_dir.glob(f"models--*{model_name.replace('/', '--')}*"):
                            print(f"Removing {model_dir}")
                            sys.stdout.flush()
                            shutil.rmtree(model_dir, ignore_errors=True)
                else:
                    # Final attempt failed
                    raise RuntimeError(f"Failed to load model after {max_retries} attempts: {e}")
        
        # Move to device if not using device_map
        if self.device == "cuda" and not hasattr(self.model, 'hf_device_map'):
            self.model = self.model.to(self.device)
        
        self.model.eval()
        
        print(f"✓ Vision model loaded successfully (GPU: {self.device == 'cuda'})")
        sys.stdout.flush()
    
    def preprocess_image(
        self,
        image: Image.Image,
        max_size: int = 1024
    ) -> Image.Image:
        """Preprocess image for the model."""
        if image.mode != "RGB":
            image = image.convert("RGB")
        
        # Qwen2-VL handles resizing internally, but we'll limit extreme sizes
        if max(image.size) > max_size:
            ratio = max_size / max(image.size)
            new_size = (int(image.size[0] * ratio), int(image.size[1] * ratio))
            image = image.resize(new_size, Image.Resampling.LANCZOS)
        
        return image
    
    def _generate_with_qwen(
        self,
        image: Image.Image,
        prompt: str,
        max_new_tokens: int = 100,
        min_new_tokens: int = 10
    ) -> str:
        """Generate caption using Qwen2-VL."""
        # Prepare messages in Qwen2-VL format
        messages = [
            {
                "role": "user",
                "content": [
                    {
                        "type": "image",
                        "image": image,
                    },
                    {"type": "text", "text": prompt},
                ],
            }
        ]
        
        # Apply chat template
        text = self.processor.apply_chat_template(
            messages, tokenize=False, add_generation_prompt=True
        )
        
        # Process vision info
        image_inputs, video_inputs = process_vision_info(messages)
        
        # Prepare inputs
        inputs = self.processor(
            text=[text],
            images=image_inputs,
            videos=video_inputs,
            padding=True,
            return_tensors="pt",
        )
        
        # Move to device
        inputs = inputs.to(self.device)
        
        # Generate
        with torch.no_grad():
            generated_ids = self.model.generate(
                **inputs,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                do_sample=False,  # Deterministic for consistency
                temperature=None,  # Not used when do_sample=False
                top_p=None,
            )
        
        # Trim input tokens and decode
        generated_ids_trimmed = [
            out_ids[len(in_ids):] for in_ids, out_ids in zip(inputs.input_ids, generated_ids)
        ]
        
        output_text = self.processor.batch_decode(
            generated_ids_trimmed,
            skip_special_tokens=True,
            clean_up_tokenization_spaces=False
        )
        
        return output_text[0].strip()
    
    def generate_caption(
        self,
        image: Image.Image,
        max_new_tokens: int = 50,
        min_new_tokens: int = 5,
        num_beams: int = 5  # Kept for API compatibility but not used
    ) -> str:
        """Generate a basic caption for the image."""
        prompt = "Describe this image in detail."
        return self._generate_with_qwen(image, prompt, max_new_tokens, min_new_tokens)
    
    def generate_conditional_caption(
        self,
        image: Image.Image,
        prompt: str,
        max_new_tokens: int = 100,
        min_new_tokens: int = 10,
        num_beams: int = 5  # Kept for API compatibility but not used
    ) -> str:
        """Generate a caption conditioned on a prompt."""
        return self._generate_with_qwen(image, prompt, max_new_tokens, min_new_tokens)
    
    def analyze_image(
        self,
        image: Image.Image,
        prompt: Optional[str] = None,
        max_new_tokens: int = 100,
        min_new_tokens: int = 10,
        num_beams: int = 5,  # Kept for API compatibility
        include_details: bool = False
    ) -> VisionAnalysisResult:
        """Analyze an image and generate detailed descriptions."""
        start_time = time.time()
        original_size = image.size
        
        processed = self.preprocess_image(image)
        
        if prompt:
            detailed_caption = self.generate_conditional_caption(
                processed,
                prompt,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                num_beams=num_beams
            )
            basic_caption = detailed_caption
        else:
            # Default prompt for basic caption
            basic_caption = self.generate_caption(
                processed,
                max_new_tokens=max_new_tokens,
                min_new_tokens=min_new_tokens,
                num_beams=num_beams
            )
            detailed_caption = basic_caption
        
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
        max_new_tokens: int = 100,
        include_details: bool = False
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
            return self.generate_conditional_caption(processed, prompt, max_new_tokens=50)
        return self.generate_caption(processed, max_new_tokens=50)
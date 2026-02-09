from fastapi import APIRouter, UploadFile, File, Depends, Query
from typing import Optional
from pydantic import BaseModel, Field
import base64
import httpx
from pathlib import Path
from PIL import Image
import io

from app.config import get_settings, Settings
from app.middleware.error_handler import ImageProcessingError, ModelError


router = APIRouter()

SUPPORTED_FORMATS = {"png", "jpg", "jpeg", "gif", "bmp", "webp", "tiff", "tif"}


def get_analyzer():
    """Get the global analyzer instance."""
    from app.main import get_analyzer as get_model
    analyzer = get_model()
    if analyzer is None:
        raise ModelError("Vision analyzer not initialized")
    return analyzer


def validate_image_format(filename: str) -> None:
    """Validate that the image format is supported."""
    if filename:
        ext = filename.rsplit(".", 1)[-1].lower()
        if ext not in SUPPORTED_FORMATS:
            raise ImageProcessingError(f"Unsupported image format: {ext}")


class AnalysisResponse(BaseModel):
    """Response model for vision analysis."""
    model_config = {"protected_namespaces": ()}
    description: str = Field(..., description="Basic image caption")
    detailed_caption: str = Field(..., description="Detailed image description")
    llm_context: str = Field(..., description="Formatted for LLM context")
    prompt_used: Optional[str] = Field(None, description="Custom prompt if provided")
    processing_time_ms: float = Field(..., description="Processing time")
    image_dimensions: dict = Field(..., description="Image dimensions")
    model_name: str = Field(..., description="Model used for analysis")


class Base64ImageRequest(BaseModel):
    """Request for analysis with base64-encoded image."""
    image: str = Field(..., description="Base64-encoded image data")
    prompt: Optional[str] = Field(None, description="Optional prompt to guide description")
    include_details: bool = Field(True, description="Generate detailed description")
    include_metadata: bool = Field(False, description="Include metadata in LLM context")


class BytesImageRequest(BaseModel):
    """Request for analysis with raw bytes array."""
    bytes: list[int] = Field(..., description="Image as array of bytes (0-255)")
    prompt: Optional[str] = Field(None, description="Optional prompt to guide description")
    include_details: bool = Field(True, description="Generate detailed description")
    include_metadata: bool = Field(False, description="Include metadata in LLM context")


class BatchImageItem(BaseModel):
    """Single image item in a batch request."""
    index: int = Field(..., description="Index/identifier for this image")
    image: Optional[str] = Field(None, description="Base64-encoded image data")
    path: Optional[str] = Field(None, description="Local file path to image")
    url: Optional[str] = Field(None, description="URL of the image")
    prompt: Optional[str] = Field(None, description="Optional prompt for this specific image")


class BatchAnalysisRequest(BaseModel):
    """Request for batch image analysis."""
    images: list[BatchImageItem] = Field(..., description="List of images to analyze")
    global_prompt: Optional[str] = Field(None, description="Default prompt for all images")
    include_details: bool = Field(True, description="Generate detailed descriptions")
    include_metadata: bool = Field(False, description="Include metadata in output")
    continue_on_error: bool = Field(True, description="Continue processing if an image fails")


class BatchImageResult(BaseModel):
    """Result for a single image in batch processing."""
    index: int = Field(..., description="Index of the image")
    success: bool = Field(..., description="Whether processing succeeded")
    description: Optional[str] = Field(None, description="Basic image caption")
    detailed_caption: Optional[str] = Field(None, description="Detailed description")
    llm_context: Optional[str] = Field(None, description="Formatted for LLM context")
    processing_time_ms: Optional[float] = Field(None, description="Processing time")
    error: Optional[str] = Field(None, description="Error message if failed")


class BatchAnalysisResponse(BaseModel):
    """Response for batch image analysis."""
    results: list[BatchImageResult] = Field(..., description="Results for each image")
    total_images: int = Field(..., description="Total images processed")
    successful: int = Field(..., description="Number of successful analyses")
    failed: int = Field(..., description="Number of failed analyses")
    total_processing_time_ms: float = Field(..., description="Total processing time")


class URLImageRequest(BaseModel):
    """Request for analysis with image URL."""
    url: str = Field(..., description="URL of the image")
    prompt: Optional[str] = Field(None, description="Optional prompt")
    include_details: bool = Field(True, description="Generate detailed description")
    include_metadata: bool = Field(False, description="Include metadata in LLM context")


def convert_to_response(result, include_metadata: bool = False) -> AnalysisResponse:
    """Convert VisionAnalysisResult to AnalysisResponse."""
    return AnalysisResponse(
        description=result.description,
        detailed_caption=result.detailed_caption,
        llm_context=result.to_llm_context(include_metadata),
        prompt_used=result.prompt_used,
        processing_time_ms=result.processing_time_ms,
        image_dimensions={
            "width": result.image_dimensions[0],
            "height": result.image_dimensions[1]
        },
        model_name=result.model_name
    )


@router.post("/analyze", response_model=AnalysisResponse)
async def analyze_uploaded_image(
    file: UploadFile = File(..., description="Image file to analyze"),
    prompt: Optional[str] = Query(None, description="Optional prompt to guide the description"),
    include_details: bool = Query(True, description="Generate detailed multi-part description"),
    include_metadata: bool = Query(False, description="Include metadata in LLM context"),
    settings: Settings = Depends(get_settings)
) -> AnalysisResponse:
    """
    Analyze an uploaded image and generate detailed descriptions.
    
    Returns descriptions that explain what's in the image, suitable for LLM context.
    
    - **file**: Image file (PNG, JPG, JPEG, GIF, BMP, WebP, TIFF)
    - **prompt**: Optional prompt to guide the description (e.g., "Describe the chart data")
    - **include_details**: Generate multiple detailed descriptions
    - **include_metadata**: Include image dimensions in output
    """
    validate_image_format(file.filename)
    
    try:
        contents = await file.read()
        analyzer = get_analyzer()
        
        result = analyzer.analyze_from_bytes(
            contents,
            prompt=prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=include_details
        )
        
        return convert_to_response(result, include_metadata)
        
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to analyze image: {str(e)}")


@router.post("/analyze/base64", response_model=AnalysisResponse)
async def analyze_base64_image(
    request: Base64ImageRequest,
    settings: Settings = Depends(get_settings)
) -> AnalysisResponse:
    """
    Analyze a base64-encoded image.
    
    - **image**: Base64-encoded image data
    - **prompt**: Optional prompt to guide description
    - **include_details**: Generate detailed description
    """
    try:
        image_data = request.image
        if "," in image_data:
            image_data = image_data.split(",", 1)[1]
        
        image_bytes = base64.b64decode(image_data)
        analyzer = get_analyzer()
        
        result = analyzer.analyze_from_bytes(
            image_bytes,
            prompt=request.prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=request.include_details
        )
        
        return convert_to_response(result, request.include_metadata)
        
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to analyze base64 image: {str(e)}")


@router.post("/analyze/bytes", response_model=AnalysisResponse)
async def analyze_bytes_image(
    request: BytesImageRequest,
    settings: Settings = Depends(get_settings)
) -> AnalysisResponse:
    """
    Analyze an image from raw bytes array.
    
    - **bytes**: Array of integers (0-255) representing image bytes
    - **prompt**: Optional prompt to guide description
    - **include_details**: Generate detailed description
    
    Example: {"bytes": [137, 80, 78, 71, ...], "prompt": "Describe this"}
    """
    try:
        # Convert list of integers to bytes
        image_bytes = bytes(request.bytes)
        analyzer = get_analyzer()
        
        result = analyzer.analyze_from_bytes(
            image_bytes,
            prompt=request.prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=request.include_details
        )
        
        return convert_to_response(result, request.include_metadata)
        
    except ValueError as e:
        raise ImageProcessingError(f"Invalid bytes array: values must be 0-255. {str(e)}")
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to analyze bytes image: {str(e)}")


@router.post("/analyze/url", response_model=AnalysisResponse)
async def analyze_url_image(
    request: URLImageRequest,
    settings: Settings = Depends(get_settings)
) -> AnalysisResponse:
    """
    Analyze an image from URL.
    
    - **url**: URL of the image
    - **prompt**: Optional prompt to guide description
    """
    try:
        async with httpx.AsyncClient(timeout=30.0) as client:
            response = await client.get(request.url)
            response.raise_for_status()
            image_bytes = response.content
        
        analyzer = get_analyzer()
        
        result = analyzer.analyze_from_bytes(
            image_bytes,
            prompt=request.prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=request.include_details
        )
        
        return convert_to_response(result, request.include_metadata)
        
    except httpx.HTTPError as e:
        raise ImageProcessingError(f"Failed to fetch image from URL: {str(e)}")
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to analyze image from URL: {str(e)}")


@router.post("/analyze/path", response_model=AnalysisResponse)
async def analyze_local_image(
    file_path: str = Query(..., description="Local file path to the image"),
    prompt: Optional[str] = Query(None, description="Optional prompt"),
    include_details: bool = Query(True, description="Generate detailed description"),
    include_metadata: bool = Query(False, description="Include metadata in LLM context"),
    settings: Settings = Depends(get_settings)
) -> AnalysisResponse:
    """
    Analyze an image from a local file path.
    
    - **file_path**: Full path to the image file on the server
    - **prompt**: Optional prompt to guide description
    """
    try:
        path = Path(file_path)
        
        if ".." in str(path):
            raise ImageProcessingError("Invalid file path: directory traversal not allowed")
        
        if not path.exists():
            raise ImageProcessingError(f"File not found: {file_path}")
        
        if not path.is_file():
            raise ImageProcessingError(f"Path is not a file: {file_path}")
        
        validate_image_format(path.name)
        
        with open(path, "rb") as f:
            image_bytes = f.read()
        
        analyzer = get_analyzer()
        
        result = analyzer.analyze_from_bytes(
            image_bytes,
            prompt=prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=include_details
        )
        
        return convert_to_response(result, include_metadata)
        
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to analyze image from path: {str(e)}")


@router.post("/describe", response_model=dict)
async def quick_describe(
    file: UploadFile = File(..., description="Image file"),
    prompt: Optional[str] = Query(None, description="Optional prompt"),
    settings: Settings = Depends(get_settings)
) -> dict:
    """
    Quick image description - faster, single caption.
    
    Returns just the description text, optimized for speed.
    """
    validate_image_format(file.filename)
    
    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents))
        
        if image.mode != "RGB":
            image = image.convert("RGB")
        
        analyzer = get_analyzer()
        description = analyzer.quick_describe(image, prompt)
        
        return {
            "description": description,
            "llm_context": f"[Image Description]\n{description}"
        }
        
    except Exception as e:
        if isinstance(e, (ImageProcessingError, ModelError)):
            raise
        raise ImageProcessingError(f"Failed to describe image: {str(e)}")


async def _process_single_batch_image(
    item: BatchImageItem,
    analyzer,
    settings: Settings,
    global_prompt: Optional[str],
    include_details: bool,
    include_metadata: bool
) -> BatchImageResult:
    """Process a single image from batch request."""
    import time
    start_time = time.time()
    
    try:
        prompt = item.prompt or global_prompt
        image_bytes = None
        
        # Get image bytes from one of the sources
        if item.image:
            # Base64 image
            image_data = item.image
            if "," in image_data:
                image_data = image_data.split(",", 1)[1]
            image_bytes = base64.b64decode(image_data)
            
        elif item.path:
            # Local file path
            path = Path(item.path)
            if ".." in str(path):
                raise ImageProcessingError("Invalid file path: directory traversal not allowed")
            if not path.exists():
                raise ImageProcessingError(f"File not found: {item.path}")
            if not path.is_file():
                raise ImageProcessingError(f"Path is not a file: {item.path}")
            validate_image_format(path.name)
            with open(path, "rb") as f:
                image_bytes = f.read()
                
        elif item.url:
            # URL
            async with httpx.AsyncClient(timeout=30.0) as client:
                response = await client.get(item.url)
                response.raise_for_status()
                image_bytes = response.content
        else:
            raise ImageProcessingError("No image source provided (image, path, or url required)")
        
        # Analyze the image
        result = analyzer.analyze_from_bytes(
            image_bytes,
            prompt=prompt,
            max_new_tokens=settings.max_new_tokens,
            include_details=include_details
        )
        
        processing_time = (time.time() - start_time) * 1000
        
        return BatchImageResult(
            index=item.index,
            success=True,
            description=result.description,
            detailed_caption=result.detailed_caption,
            llm_context=result.to_llm_context(include_metadata),
            processing_time_ms=processing_time,
            error=None
        )
        
    except Exception as e:
        processing_time = (time.time() - start_time) * 1000
        return BatchImageResult(
            index=item.index,
            success=False,
            description=None,
            detailed_caption=None,
            llm_context=None,
            processing_time_ms=processing_time,
            error=str(e)
        )


@router.post("/analyze/batch", response_model=BatchAnalysisResponse)
async def analyze_batch_images(
    request: BatchAnalysisRequest,
    settings: Settings = Depends(get_settings)
) -> BatchAnalysisResponse:
    """
    Analyze multiple images in a single request.
    
    Each image can be provided as:
    - **image**: Base64-encoded data
    - **path**: Local file path (e.g., /data/image.png)
    - **url**: Image URL
    
    Each image has an **index** field to identify it in results.
    
    Example request:
    ```json
    {
        "images": [
            {"index": 1, "path": "/data/chart1.png", "prompt": "Describe this chart"},
            {"index": 2, "path": "/data/diagram.png"},
            {"index": 3, "url": "https://example.com/image.jpg"}
        ],
        "global_prompt": "Explain what is shown",
        "include_details": true,
        "continue_on_error": true
    }
    ```
    """
    import time
    total_start = time.time()
    
    analyzer = get_analyzer()
    results: list[BatchImageResult] = []
    
    for item in request.images:
        result = await _process_single_batch_image(
            item=item,
            analyzer=analyzer,
            settings=settings,
            global_prompt=request.global_prompt,
            include_details=request.include_details,
            include_metadata=request.include_metadata
        )
        results.append(result)
        
        # Stop on first error if continue_on_error is False
        if not result.success and not request.continue_on_error:
            break
    
    total_time = (time.time() - total_start) * 1000
    successful = sum(1 for r in results if r.success)
    failed = sum(1 for r in results if not r.success)
    
    return BatchAnalysisResponse(
        results=results,
        total_images=len(results),
        successful=successful,
        failed=failed,
        total_processing_time_ms=total_time
    )

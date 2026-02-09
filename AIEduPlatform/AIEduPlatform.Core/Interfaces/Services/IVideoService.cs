using AIEduPlatform.Core.DTOs.Video;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IVideoService
    {
        Task<VideoAnalysisResponse> AnalyzeVideoAsync(
                    Stream videoStream,
                    VideoAnalysisRequest request,
                    string? fileName = null,
                    CancellationToken ct = default);
    }
}


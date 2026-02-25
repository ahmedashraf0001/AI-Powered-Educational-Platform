using AIEduPlatform.Core.DTOs.Vision;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IVisionService
    {
        Task<VisionAnalysisResponse> ExtractInfoFromImageAsync(Stream imageData, CancellationToken ct = default);
    }
}

using AIEduPlatform.Core.DTOs.ML_Health;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Monitors
{
    public interface IAIServiceHealthMonitor
    {
        Task<DetailedHealthResponse> GetEmbeddingServiceHealthAsync();
        Task<DetailedHealthResponse> GetRerankingServiceHealthAsync();
        Task<bool> IsEmbeddingServiceReadyAsync();
        Task<bool> IsRerankingServiceReadyAsync();
        Task<ServiceStatus> GetOverallStatusAsync();
    }
}

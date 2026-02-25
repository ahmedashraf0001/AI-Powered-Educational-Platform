using AIEduPlatform.Core.DTOs.Reranking;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IRerankingService
    {
        Task<RerankResponse> RerankAsync(RerankRequest request, CancellationToken ct);
    }
}

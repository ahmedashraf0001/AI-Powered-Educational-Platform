using AIEduPlatform.Core.DTOs.Embedding;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IEmbeddingService
    {
        Task<BatchEmbeddingResponse> GetBatchEmbeddingAsync(BatchEmbeddingRequest request, CancellationToken ct);
        Task<EmbeddingResponse> GetEmbeddingAsync(EmbeddingRequest request, CancellationToken ct);

    }
}

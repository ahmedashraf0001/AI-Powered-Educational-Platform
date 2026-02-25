using AIEduPlatform.Core.DTOs.Concept;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IGraphMergeService
    {
        Task MergeAndStoreGraphAsync(
            Guid courseId,
            Guid materialId,
            List<ChunkConceptsResult> extractions,
            IEmbeddingService embeddingService,
            CancellationToken ct = default);
    }
}

using AIEduPlatform.Core.DTOs.Concept;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IConceptExtractionService
    {
        Task<ChunkConceptsResult> ExtractFromChunkAsync(
            string chunkContent,
            Guid chunkId,
            CancellationToken ct = default);
    }
}

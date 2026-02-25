using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Materials
{
    public class MaterialSearchResult
    {
        public Guid MaterialId { get; set; } = default!;
        public List<SearchedChunk> TopChunks { get; set; } = new();
    }
    public class SearchedChunk
    {
        public MaterialChunk Chunk { get; set; }
        public float SimilarityScore { get; set; }
    }
}

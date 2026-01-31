using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankRequest
    {
        public string Query { get; set; } = string.Empty;
        public List<RerankChunk> Chunks { get; set; } = new List<RerankChunk>();
        public int? TopK { get; set; }
        public bool Return_Documents { get; set; } = true;
    }
    public class RerankChunk
    {
        public int Index { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class BatchEmbeddingResponse
    {
        public List<EmbeddingResult> Results { get; set; } = new();
        public int TotalChunks { get; set; }
        public int Successful { get; set; }
        public int Failed { get; set; }
        public int Dimension { get; set; }
        public string Model { get; set; }
        public List<ErrorSummary> ErrorsSummary { get; set; } = new();
    }

    public class EmbeddingResult
    {
        public int Index { get; set; }
        public bool Success { get; set; }
        public List<float>? Embedding { get; set; }
        public string? Error { get; set; }
        public int TextLength { get; set; }
        public bool WasTruncated { get; set; }
    }

    public class ErrorSummary
    {
        public int Index { get; set; }
        public string Error { get; set; }
        public string TextPreview { get; set; }
    }
}

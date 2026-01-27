using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class BatchEmbeddingResponse
    {
        public List<List<float>> Embeddings { get; set; } = new List<List<float>>();
        public int Count { get; set; }
        public int Dimension { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}

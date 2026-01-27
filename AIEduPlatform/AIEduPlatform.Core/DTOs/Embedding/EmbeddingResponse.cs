using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class EmbeddingResponse
    {
        public List<float> Embedding { get; set; } = new List<float>();
        public int Dimension { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class BatchEmbeddingRequest
    {
        public List<string> Texts { get; set; } = new List<string>();
        public bool Normalize { get; set; } = true;
        public int? BatchSize { get; set; }
    }
}

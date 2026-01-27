using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Embedding
{
    public class EmbeddingRequest
    {
        public string Text { get; set; } = string.Empty;
        public bool Normalize { get; set; } = true;
    }

}

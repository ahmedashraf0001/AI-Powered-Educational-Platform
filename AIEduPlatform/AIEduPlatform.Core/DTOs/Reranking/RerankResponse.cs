using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankResponse
    {
        public List<RerankResult> Results { get; set; } = new List<RerankResult>();
        public int Count { get; set; }
        public string Model { get; set; } = string.Empty;
    }

    public class RerankResult
    {
        public int Index { get; set; }
        public float Score { get; set; }
        public string Document { get; set; } = string.Empty;    
    }
}

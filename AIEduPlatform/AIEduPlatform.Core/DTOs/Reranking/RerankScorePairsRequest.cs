using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankScorePairsRequest
    {
        public List<Pair> Pairs { get; set; } = new List<Pair>();
        public int BatchSize { get; set; }
    }

    public class Pair
    {
        public string Query { get; set; } = string.Empty;
        public string Passage { get; set; } = string.Empty;
    }

}

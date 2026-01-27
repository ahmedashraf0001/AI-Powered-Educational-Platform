using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankScorePairsResponse
    {
        public List<Result> Results { get; set; } = new List<Result>();
        public int Count { get; set; }
        public string Model { get; set; } = string.Empty;
    }

    public class Result
    {
        public int Index { get; set; }
        public float Score { get; set; }
        public string Document { get; set; } = string.Empty;
    }
}

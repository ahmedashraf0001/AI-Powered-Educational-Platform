using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Reranking
{
    public class RerankRequest
    {
        public string Query { get; set; } = string.Empty;
        public List<string> Passages { get; set; } = new List<string>();
        public int? TopK { get; set; }
        public bool Return_Documents { get; set; } = true;
    }



}

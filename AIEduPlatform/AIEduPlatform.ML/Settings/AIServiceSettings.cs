using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Configurations
{
    public class AIServiceSettings
    {
        public BaseUrlsSettings BaseUrls { get; set; }
        public EmbeddingSettings Embeddings { get; set; }
        public RerankerSettings Reranker { get; set; }
        public TimeoutSettings Timeouts { get; set; }
        public RetrySettings Retry { get; set; }
    }  
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.ML.Settings
{
    public class RagSettings
    {
        public int MaxRetryAttempts { get; set; }
        public int EmbeddingDelayMs { get; set; }
        public float MinRerankScore { get; set; }
        public double MaxAcceptableFailureRatio { get; set; }
    }

}

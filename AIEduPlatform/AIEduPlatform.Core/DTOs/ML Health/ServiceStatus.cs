
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class ServiceStatus
    {
        public bool EmbeddingServiceReady { get; set; }
        public bool RerankingServiceReady { get; set; }
        public bool IsFullyOperational { get; set; }
        public DateTime Timestamp { get; set; }
    }
}

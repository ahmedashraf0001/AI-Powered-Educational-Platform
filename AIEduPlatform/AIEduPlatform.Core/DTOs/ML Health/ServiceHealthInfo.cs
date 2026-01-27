using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class ServiceHealthInfo
    {
        public bool IsHealthy { get; set; }
        public string Status { get; set; }
        public long ResponseTimeMs { get; set; }
        public string ServiceName { get; set; }
        public string Url { get; set; }
        public string ErrorMessage { get; set; }
    }
}

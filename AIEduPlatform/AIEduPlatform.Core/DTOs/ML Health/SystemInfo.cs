namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class SystemInfo
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryTotalGb { get; set; }
        public double MemoryUsedGb { get; set; }
        public double MemoryPercent { get; set; }
        public string Timestamp { get; set; }
    }
}

namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class DetailedHealthResponse
    {
        public string Status { get; set; }
        public ModelInfo Model { get; set; }
        public SystemInfo System { get; set; }
        public GpuInfo Gpu { get; set; }
        public Dictionary<string, object> Config { get; set; }
    }
}

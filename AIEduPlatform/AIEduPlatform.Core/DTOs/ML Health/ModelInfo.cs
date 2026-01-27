namespace AIEduPlatform.Core.DTOs.ML_Health
{
    public class ModelInfo
    {
        public string Name { get; set; }
        public int? Dimension { get; set; }
        public int? MaxSeqLength { get; set; }
        public int? MaxLength { get; set; }
        public string Device { get; set; }
        public bool IsCudaAvailable { get; set; }
    }
}

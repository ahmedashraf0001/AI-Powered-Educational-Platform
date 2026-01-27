namespace AIEduPlatform.Application.Common.Models
{
    public class ErrorResponse
    {
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

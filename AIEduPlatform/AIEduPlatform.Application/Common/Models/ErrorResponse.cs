namespace AIEduPlatform.Application.Common.Models
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;
        public string? Message { get; set; }
        public ErrorDetail? Error { get; set; }
    }

    public class ErrorDetail
    {
        public string Code { get; set; } = string.Empty;
        public int Status { get; set; }
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}

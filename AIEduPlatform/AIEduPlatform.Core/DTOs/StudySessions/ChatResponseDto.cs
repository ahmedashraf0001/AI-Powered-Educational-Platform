namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record ChatResponseDto
    {
        public string Response { get; init; } = string.Empty;
        public List<string>? Sources { get; init; }
    }
}

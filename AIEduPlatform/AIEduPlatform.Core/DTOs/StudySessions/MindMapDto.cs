namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record MindMapDto
    {
        public Guid Id { get; init; }
        public string Topic { get; init; } = string.Empty;
        public string Nodes { get; init; } = string.Empty;
        public string Connections { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}

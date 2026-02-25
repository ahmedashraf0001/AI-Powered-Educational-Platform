namespace AIEduPlatform.Core.Domain.Entities
{
    public class Review : BaseEntity
    {
        public Guid CourseId { get; set; }
        public Guid StudentId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public Course Course { get; set; } = null!;
        public User Student { get; set; } = null!;
    }
}

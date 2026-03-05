namespace AIEduPlatform.Core.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<CourseCategory> CourseCategories { get; set; } = [];
    }
}

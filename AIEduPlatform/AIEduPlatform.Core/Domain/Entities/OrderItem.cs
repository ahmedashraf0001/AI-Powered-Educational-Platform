namespace AIEduPlatform.Core.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid CourseId { get; set; }
        public decimal Price { get; set; }

        public Order Order { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}

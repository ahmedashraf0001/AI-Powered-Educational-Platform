namespace AIEduPlatform.Core.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Guid CourseId { get; set; }
        public decimal PriceAtTimeOfAdding { get; set; }
        public DateTime AddedAt { get; set; }

        public Cart Cart { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}

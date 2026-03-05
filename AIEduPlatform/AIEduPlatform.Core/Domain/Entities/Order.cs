using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid? CartId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "usd";
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string? StripePaymentIntentId { get; set; }
        public string? StripePaymentIntentClientSecret { get; set; }
        public DateTime? PaidAt { get; set; }

        public User User { get; set; } = null!;
        public Cart? Cart { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}

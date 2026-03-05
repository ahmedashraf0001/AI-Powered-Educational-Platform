using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Guid UserId { get; set; }
        public CartStatus Status { get; set; } = CartStatus.Active;

        public User User { get; set; } = null!;
        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}

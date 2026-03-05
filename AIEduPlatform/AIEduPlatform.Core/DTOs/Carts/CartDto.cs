namespace AIEduPlatform.Core.DTOs.Carts
{
    public record CartDto
    {
        public Guid CartId { get; init; }
        public List<CartItemDto> Items { get; init; } = [];
        public int ItemCount { get; init; }
        public decimal Subtotal { get; init; }
        public string Currency { get; init; } = "usd";
    }
}

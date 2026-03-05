using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken ct = default);
        Task<Order?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default);
        Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}

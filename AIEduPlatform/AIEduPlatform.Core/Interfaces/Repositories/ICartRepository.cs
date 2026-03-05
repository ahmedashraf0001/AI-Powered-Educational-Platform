using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ICartRepository : IGenericRepository<Cart>
    {
        Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Cart?> GetCartWithItemsAsync(Guid userId, CancellationToken ct = default);
    }
}

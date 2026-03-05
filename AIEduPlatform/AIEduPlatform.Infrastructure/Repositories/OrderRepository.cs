using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        public OrderRepository(AppDbContext context) : base(context) { }

        public async Task<Order?> GetByIdWithItemsAsync(Guid orderId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Course)
                .FirstOrDefaultAsync(o => o.Id == orderId, ct);
        }

        public async Task<Order?> GetByStripePaymentIntentIdAsync(string paymentIntentId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Course)
                .FirstOrDefaultAsync(o => o.StripePaymentIntentId == paymentIntentId, ct);
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }
    }
}

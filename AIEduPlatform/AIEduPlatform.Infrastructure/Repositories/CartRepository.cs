using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class CartRepository : GenericRepository<Cart>, ICartRepository
    {
        public CartRepository(AppDbContext context) : base(context) { }

        public async Task<Cart?> GetActiveCartByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Course)
                        .ThenInclude(course => course.Teacher)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, ct);
        }

        public async Task<Cart?> GetCartWithItemsAsync(Guid userId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Course)
                        .ThenInclude(course => course.Teacher)
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == CartStatus.Active, ct);
        }
    }
}

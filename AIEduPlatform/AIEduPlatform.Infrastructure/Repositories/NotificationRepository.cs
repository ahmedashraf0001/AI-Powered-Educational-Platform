using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context) { }

        public async Task<List<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, bool unreadOnly = false, CancellationToken ct = default)
        {
            var query = _dbSet.Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);
        }

        public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
        {
            return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead, ct);
        }

        public async Task<int> GetTotalCountAsync(Guid userId, bool unreadOnly = false, CancellationToken ct = default)
        {
            var query = _dbSet.Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query.CountAsync(ct);
        }

        public async Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
        {
            await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.ReadAt, DateTime.UtcNow), ct);
        }
    }
}

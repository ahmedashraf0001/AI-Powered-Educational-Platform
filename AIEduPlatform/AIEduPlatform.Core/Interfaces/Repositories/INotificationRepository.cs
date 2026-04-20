using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<Notification>> GetByUserIdAsync(Guid userId, int page, int pageSize, bool unreadOnly = false, CancellationToken ct = default);
        Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);
        Task<int> GetTotalCountAsync(Guid userId, bool unreadOnly = false, CancellationToken ct = default);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);
        Task DeleteAllByUserIdAsync(Guid userId, CancellationToken ct = default);
    }
}

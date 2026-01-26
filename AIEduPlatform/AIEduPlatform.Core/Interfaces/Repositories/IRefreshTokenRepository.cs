using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(Guid userId);
        Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);
        Task<bool> IsTokenValidAsync(string token);
        Task RevokeTokenAsync(string token);
        Task RevokeAllUserTokensAsync(Guid userId);
        Task DeleteExpiredAndRevokedTokensAsync();
        Task<int> GetActiveSessionCountAsync(Guid userId);
    }
}

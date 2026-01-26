using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task DeleteExpiredAndRevokedTokensAsync()
        {
            var tokensToDelete = _dbSet.Where(token => token.IsRevoked || token.ExpiryTime <= DateTime.UtcNow);
            DeleteRange(tokensToDelete);
            await SaveAsync();
        }

        public Task<int> GetActiveSessionCountAsync(Guid userId)
        {
            return _dbSet.CountAsync(token => token.UserId == userId && !token.IsRevoked && token.ExpiryTime > DateTime.UtcNow);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(token => token.UserId == userId && !token.IsRevoked && token.ExpiryTime > DateTime.UtcNow)
                .ToListAsync();
        }

        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return _dbSet.FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetTokensByUserIdAsync(Guid userId)
        {
            return await FindAsync(t => t.UserId == userId);
        }

        public Task<bool> IsTokenValidAsync(string token)
        {
            return AnyAsync(t => t.Token == token && !t.IsRevoked && t.ExpiryTime > DateTime.UtcNow);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var userTokens = await GetTokensByUserIdAsync(userId);
            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }
            UpdateRange(userTokens);
            await SaveAsync();
        }

        public async Task RevokeTokenAsync(string token)
        {
            var tokenEntity = await GetByTokenAsync(token);
            if (tokenEntity != null)
            {
                tokenEntity.IsRevoked = true;
                Update(tokenEntity);
                await SaveAsync();
            }
        }
    }
}

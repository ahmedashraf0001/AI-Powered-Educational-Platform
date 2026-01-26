using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);
        
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task SaveAsync();
    }
}

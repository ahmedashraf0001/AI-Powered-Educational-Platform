using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Category?> GetByNameAsync(string name, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), ct);
        }

        public async Task<List<Category>> GetCategoriesByCourseAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _context.Set<CourseCategory>()
                .Where(cc => cc.CourseId == courseId)
                .Select(cc => cc.Category)
                .ToListAsync(ct);
        }

        public async Task<List<Category>> SearchCategoriesAsync(string searchTerm, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(c => c.Name.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync(ct);
        }
    }
}

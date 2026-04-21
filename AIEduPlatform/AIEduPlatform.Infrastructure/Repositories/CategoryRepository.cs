using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Categories;
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
        public async Task<Dictionary<Guid, int>> GetCourseCountsByCategoryIdsAsync(IEnumerable<Guid> categoryIds, CancellationToken ct = default)
        {
            return await _context.Set<CourseCategory>()
                .Where(cc => categoryIds.Contains(cc.CategoryId))
                .GroupBy(cc => cc.CategoryId)
                .Select(g => new { CategoryId = g.Key, CourseCount = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.CourseCount, ct);
        }

        public async Task<List<CategoryDto>> GetCategoryDtosAsync(string? searchTerm, CancellationToken ct = default)
        {
            var query = _dbSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowered = searchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowered));
            }

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    CourseCount = c.CourseCategories.Count(),
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync(ct);
        }
    }
}

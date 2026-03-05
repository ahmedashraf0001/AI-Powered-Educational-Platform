using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ICategoryRepository : IGenericRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name, CancellationToken ct = default);
        Task<List<Category>> GetCategoriesByCourseAsync(Guid courseId, CancellationToken ct = default);
        Task<List<Category>> SearchCategoriesAsync(string searchTerm, CancellationToken ct = default);
    }
}

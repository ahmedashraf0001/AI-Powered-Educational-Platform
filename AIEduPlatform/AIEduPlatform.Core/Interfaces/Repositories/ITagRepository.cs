using AIEduPlatform.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ITagRepository : IGenericRepository<Tag>
    {

        // =========================
        // Tag-specific operations
        // =========================

        Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<List<Tag>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

        Task<List<Tag>> SearchAsync(string query, int take = 20, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets existing tags or creates missing ones (used by NLP pipeline).
        /// </summary>
        Task<List<Tag>> GetOrCreateAsync(IEnumerable<string> tagNames, CancellationToken cancellationToken = default);
        Task<List<Tag>> GetAllByIdsAsync(IEnumerable<Guid> TagIds, CancellationToken ct = default);
    }
}

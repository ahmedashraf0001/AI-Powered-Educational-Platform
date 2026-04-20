using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class TagRepository : GenericRepository<Tag>, ITagRepository
    {
        private readonly AppDbContext _ctx;

        public TagRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<List<Tag>> GetAllByIdsAsync(IEnumerable<Guid> TagIds, CancellationToken ct = default)
        {
            return await _ctx.Tags.Where(e => TagIds.Contains(e.Id)).ToListAsync(ct);
        }
        public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            name = Normalize(name);

            return await _ctx.Tags
                .FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
        }

        public async Task<List<Tag>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
        {
            var normalized = names.Select(Normalize).ToList();

            return await _ctx.Tags
                .Where(t => normalized.Contains(t.Name))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<Tag>> GetOrCreateAsync(IEnumerable<string> tagNames, CancellationToken cancellationToken = default)
        {
            var normalized = tagNames
                .Select(Normalize)
                .Distinct()
                .ToList();

            var existingTags = await GetByNamesAsync(normalized, cancellationToken);

            var existingTagSet = existingTags
                .Select(t => t.Name)
                .ToHashSet();

            var newTags = normalized
                .Where(name => !existingTagSet.Contains(name))
                .Select(name => new Tag
                {
                    Name = name,
                    DisplayName = name
                })
                .ToList();

            if (newTags.Any())
            {
                await _ctx.Tags.AddRangeAsync(newTags, cancellationToken);
            }

            return existingTags.Concat(newTags).ToList();
        }

        public async Task<List<Tag>> SearchAsync(string query, int take = 20, CancellationToken cancellationToken = default)
        {
            query = Normalize(query);

            return await _ctx.Tags
                .Where(t => t.Name.Contains(query))
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        private static string Normalize(string input)
            => input.Trim().ToLower();


    }
}

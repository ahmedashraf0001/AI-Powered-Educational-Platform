using AIEduPlatform.Core.Interfaces.Repositories;

namespace AIEduPlatform.Application.Common.Services
{
    internal sealed class RecommendationDiversityEngine
    {
        private readonly IUnitOfWork _unitOfWork;

        public RecommendationDiversityEngine(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Guid>> SelectDiverseTopCourseIdsAsync(
            IReadOnlyList<Guid> rankedCourseIds,
            int take,
            int maxPerPrimaryTag,
            CancellationToken ct)
        {
            if (rankedCourseIds.Count == 0 || take <= 0)
                return new List<Guid>();

            if (maxPerPrimaryTag <= 0)
                return rankedCourseIds.Take(take).ToList();

            var diversifiedRanking = await ApplyTagClusterDiversityAsync(rankedCourseIds, maxPerPrimaryTag, ct);

            return diversifiedRanking
                .Take(take)
                .ToList();
        }

        public async Task<List<Guid>> ApplyTagClusterDiversityAsync(
            IReadOnlyList<Guid> rankedCourseIds,
            int maxPerPrimaryTag,
            CancellationToken ct)
        {
            if (rankedCourseIds.Count == 0 || maxPerPrimaryTag <= 0)
                return rankedCourseIds.ToList();

            var tagRows = await _unitOfWork.Courses.GetCourseTagsAsync(rankedCourseIds, ct);
            var courseTags = tagRows.ToDictionary(
                x => x.CourseId,
                x => (x.TagIds ?? new List<Guid>())
                    .Where(id => id != Guid.Empty)
                    .Distinct()
                    .ToList());

            var tagFrequency = courseTags
                .SelectMany(kv => kv.Value)
                .GroupBy(id => id)
                .ToDictionary(g => g.Key, g => g.Count());

            var primaryTagByCourse = courseTags.ToDictionary(
                kv => kv.Key,
                kv => kv.Value
                    .OrderByDescending(tagId => tagFrequency.GetValueOrDefault(tagId, 0))
                    .ThenBy(tagId => tagId)
                    .FirstOrDefault());

            var prioritized = new List<Guid>(rankedCourseIds.Count);
            var overflow = new List<Guid>();
            var tagCounts = new Dictionary<Guid, int>();

            foreach (var courseId in rankedCourseIds)
            {
                var primaryTag = primaryTagByCourse.GetValueOrDefault(courseId, Guid.Empty);

                if (primaryTag == Guid.Empty)
                {
                    prioritized.Add(courseId);
                    continue;
                }

                var currentCount = tagCounts.GetValueOrDefault(primaryTag);

                if (currentCount < maxPerPrimaryTag)
                {
                    tagCounts[primaryTag] = currentCount + 1;
                    prioritized.Add(courseId);
                }
                else
                {
                    overflow.Add(courseId);
                }
            }

            prioritized.AddRange(overflow);
            return prioritized;
        }
    }
}

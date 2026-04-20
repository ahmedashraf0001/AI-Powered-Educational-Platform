using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Common.Services
{
    internal sealed class RecommendationCandidateGenerator
    {
        private const int ColdStartShuffleSeedSalt = 4001;

        private readonly IUnitOfWork _unitOfWork;
        private readonly CandidateGenerationDto _candidateConfigurations;
        private readonly ILogger<RecommendationCandidateGenerator> _logger;

        public RecommendationCandidateGenerator(
            IUnitOfWork unitOfWork,
            CandidateGenerationDto candidateConfigurations,
            ILogger<RecommendationCandidateGenerator> logger)
        {
            _unitOfWork = unitOfWork;
            _candidateConfigurations = candidateConfigurations;
            _logger = logger;
        }

        public async Task<List<Guid>> FindCandidateCoursesAsync(Guid userId, int take, CancellationToken ct = default)
        {
            if (take <= 0)
                return new List<Guid>();

            var configuredCandidateLimit = _candidateConfigurations.CandidateLimit <= 0
                ? 100
                : _candidateConfigurations.CandidateLimit;

            var candidateLimit = Math.Max(1, Math.Min(take, configuredCandidateLimit));

            // IUnitOfWork wraps a shared DbContext, so run sequentially.
            var user = await _unitOfWork.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: true,
                includeTaughtCourses: false,
                includeUserTags: false,
                ct);

            var userTags = await _unitOfWork.Users.GetUserTagsAsync(userId, ct);

            var excludedCourseIds = (user?.Enrollments ?? Enumerable.Empty<Enrollment>())
                .Where(e => e.Status != EnrollmentStatus.Dropped)
                .Select(e => e.CourseId)
                .ToHashSet();

            var hasSimilaritySignal = user?.TagEmbedding != null || userTags.Any(t => t.Weight > 0);

            var similarRatio = Math.Clamp(_candidateConfigurations.SimilarRatio, 0d, 1d);
            var popularRatio = Math.Clamp(_candidateConfigurations.PopularRatio, 0d, 1d);
            var newRatio = Math.Clamp(_candidateConfigurations.NewRatio, 0d, 1d);

            var similarTake = hasSimilaritySignal
                ? Math.Max(1, (int)Math.Floor(candidateLimit * similarRatio))
                : 0;
            var popularTake = Math.Max(0, (int)Math.Floor(candidateLimit * popularRatio));
            var newTake = Math.Max(0, (int)Math.Floor(candidateLimit * newRatio));

            var allocated = similarTake + popularTake + newTake;
            if (allocated < candidateLimit)
            {
                var remaining = candidateLimit - allocated;

                if (hasSimilaritySignal)
                    similarTake += remaining;
                else
                    popularTake += remaining;
            }

            var similarCourses = similarTake > 0
                ? await FindSimilarCoursesToUserInterest(user, userTags, excludedCourseIds, similarTake, ct)
                : new List<Guid>();

            var popularCourses = popularTake > 0
                ? await _unitOfWork.Courses.GetTopPopularCoursesAsync(popularTake, ct)
                : new List<Guid>();

            var newCourses = newTake > 0
                ? await _unitOfWork.Courses.GetNewestCoursesAsync(newTake, ct)
                : new List<Guid>();

            var candidates = similarCourses
                .Concat(popularCourses)
                .Concat(newCourses)
                .Where(id => id != Guid.Empty && !excludedCourseIds.Contains(id))
                .Distinct()
                .Take(candidateLimit)
                .ToList();

            if (candidates.Any())
                return candidates;

            return await GetColdStartCandidatesAsync(userId, excludedCourseIds, candidateLimit, ct);
        }

        private async Task<List<Guid>> FindSimilarCoursesToUserInterest(
            User? user,
            IReadOnlyList<UserTagDto> userTags,
            HashSet<Guid> excludedCourseIds,
            int take,
            CancellationToken ct = default)
        {
            if (take <= 0)
                return new List<Guid>();

            var tagIds = userTags
                .Where(t => t.Weight > 0)
                .OrderByDescending(t => t.Weight)
                .Select(t => t.TagId)
                .Distinct()
                .ToList();

            if (!tagIds.Any())
                return new List<Guid>();

            var fetchSize = Math.Min(Math.Max(take * 3, take), 200);

            var tagCandidates = await _unitOfWork.Courses
                .GetCoursesBySimilarTagsAsync(tagIds, fetchSize, ct);

            var filteredTagCandidates = tagCandidates
                .Where(id => id != Guid.Empty && !excludedCourseIds.Contains(id))
                .Distinct()
                .ToList();

            if (!filteredTagCandidates.Any())
                return new List<Guid>();

            if (user?.TagEmbedding == null)
                return filteredTagCandidates.Take(take).ToList();

            var embeddingScores = await _unitOfWork.Courses
                .GetSimilarityScoresAsync(user.TagEmbedding, filteredTagCandidates, ct);

            return filteredTagCandidates
                .OrderByDescending(id => Math.Clamp(embeddingScores.GetValueOrDefault(id), 0d, 1d))
                .ThenBy(id => id)
                .Take(take)
                .ToList();
        }

        private async Task<List<Guid>> GetColdStartCandidatesAsync(
            Guid userId,
            HashSet<Guid> excludedCourseIds,
            int take,
            CancellationToken ct)
        {
            if (take <= 0)
                return new List<Guid>();

            var fetchSize = Math.Max(take, 20);

            var popularCourses = await _unitOfWork.Courses.GetTopPopularCoursesAsync(fetchSize, ct);
            var newestCourses = await _unitOfWork.Courses.GetNewestCoursesAsync(fetchSize, ct);

            var blended = RecommendationRandomUtils
                .InterleaveLists(popularCourses, newestCourses)
                .Where(id => !excludedCourseIds.Contains(id))
                .Distinct()
                .ToList();

            var shuffled = RecommendationRandomUtils
                .ShuffleDeterministically(blended, userId, ColdStartShuffleSeedSalt)
                .Take(take)
                .ToList();

            _logger.LogInformation(
                "Cold-start candidate fallback used for user {UserId}. RequestedTake: {Take}, Returned: {Count}",
                userId,
                take,
                shuffled.Count);

            return shuffled;
        }
    }
}
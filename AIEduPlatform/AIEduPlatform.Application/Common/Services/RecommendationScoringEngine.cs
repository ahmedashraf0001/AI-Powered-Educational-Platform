using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Common.Services
{
    internal sealed class RecommendationScoringEngine
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly RecommendationWeightsDto _recommendConfigurations;
        private readonly ILogger<RecommendationScoringEngine> _logger;

        public RecommendationScoringEngine(
            IUnitOfWork unitOfWork,
            RecommendationWeightsDto recommendConfigurations,
            ILogger<RecommendationScoringEngine> logger)
        {
            _unitOfWork = unitOfWork;
            _recommendConfigurations = recommendConfigurations;
            _logger = logger;
        }

        public async Task<List<CourseScoreDto>> ScoreCoursesAsync(
            Guid userId,
            IEnumerable<Guid> candidateCourseIds,
            CancellationToken ct = default)
        {
            var ids = candidateCourseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return new List<CourseScoreDto>();

            // IUnitOfWork uses a shared DbContext per request/scope.
            var similarityScores = await CalculateSimilarityScores(ids, userId, ct);
            var popularityScores = await CalculatePopularityScores(ids, ct);
            var qualityScores = await CalculateQualityScores(ids, ct);
            var recencyScores = await CalculateRecencyScores(ids, ct);

            var result = new List<CourseScoreDto>(ids.Count);

            foreach (var id in ids)
            {
                var sim = similarityScores.GetValueOrDefault(id);
                var pop = popularityScores.GetValueOrDefault(id);
                var qual = qualityScores.GetValueOrDefault(id);
                var rec = recencyScores.GetValueOrDefault(id);

                result.Add(new CourseScoreDto
                {
                    CourseId = id,
                    Similarity = sim,
                    Popularity = pop,
                    Quality = qual,
                    Recency = rec,
                    FinalScore = CalculateFinalScore(sim, qual, pop, rec)
                });
            }

            return result;
        }

        private double CalculateFinalScore(double similarity, double quality, double popularity, double recency)
        {
            return
                (_recommendConfigurations.SimilarityWeight * similarity) +
                (_recommendConfigurations.PopularityWeight * popularity) +
                (_recommendConfigurations.QualityWeight * quality) +
                (_recommendConfigurations.RecencyWeight * recency);
        }

        private async Task<Dictionary<Guid, double>> CalculateSimilarityScores(
            IReadOnlyList<Guid> courseIds,
            Guid userId,
            CancellationToken ct = default)
        {
            if (!courseIds.Any())
                return new Dictionary<Guid, double>();

            var user = await _unitOfWork.Users.GetUserByIdAsync(
                userId,
                includeEnrollments: false,
                includeTaughtCourses: false,
                includeUserTags: false,
                ct);

            var userTags = await _unitOfWork.Users.GetUserTagsAsync(userId, ct);

            var weightedUserTags = userTags
                .Where(ut => ut.Weight > 0)
                .ToList();

            var tagWeightByTagId = weightedUserTags
                .GroupBy(ut => ut.TagId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Weight));

            var tagWeightTotal = tagWeightByTagId.Values.Sum();
            var hasTagSignal = tagWeightByTagId.Count > 0 && tagWeightTotal > 0;
            var hasCosineSignal = user?.TagEmbedding != null;

            var cosineByCourse = courseIds.ToDictionary(id => id, _ => 0d);
            if (hasCosineSignal)
            {
                var cosineScores = await _unitOfWork.Courses.GetSimilarityScoresAsync(user!.TagEmbedding!, courseIds, ct);
                foreach (var (courseId, cosine) in cosineScores)
                    cosineByCourse[courseId] = Math.Clamp(cosine, 0d, 1d);
            }

            var tagOverlapByCourse = courseIds.ToDictionary(id => id, _ => 0d);
            if (hasTagSignal)
            {
                var courseTags = await _unitOfWork.Courses.GetCourseTagsAsync(courseIds, ct);

                foreach (var courseTag in courseTags)
                {
                    var tagIds = (courseTag.TagIds ?? new List<Guid>())
                        .Where(id => id != Guid.Empty)
                        .Distinct()
                        .ToList();

                    if (!tagIds.Any())
                        continue;

                    var sharedWeight = tagIds
                        .Where(tagWeightByTagId.ContainsKey)
                        .Sum(tagId => tagWeightByTagId[tagId]);

                    if (sharedWeight <= 0)
                        continue;

                    tagOverlapByCourse[courseTag.CourseId] = Math.Clamp(sharedWeight / tagWeightTotal, 0d, 1d);
                }
            }

            var similarityByCourse = courseIds.ToDictionary(
                id => id,
                id => CombineSimilaritySignals(
                    cosineByCourse.GetValueOrDefault(id),
                    hasCosineSignal,
                    tagOverlapByCourse.GetValueOrDefault(id),
                    hasTagSignal));

            _logger.LogInformation(
                "Similarity scoring completed for user {UserId}. Courses={CourseCount}, HasCosineSignal={HasCosineSignal}, HasTagSignal={HasTagSignal}",
                userId,
                courseIds.Count,
                hasCosineSignal,
                hasTagSignal);

            return similarityByCourse;
        }

        private double CombineSimilaritySignals(
            double cosine,
            bool hasCosineSignal,
            double tagOverlap,
            bool hasTagSignal)
        {
            var cosineWeight = hasCosineSignal ? Math.Max(0d, _recommendConfigurations.SimilarityCosineWeight) : 0d;
            var tagWeight = hasTagSignal ? Math.Max(0d, _recommendConfigurations.SimilarityTagOverlapWeight) : 0d;

            var totalWeight = cosineWeight + tagWeight;
            if (totalWeight <= 0d)
                return 0d;

            var score =
                (cosineWeight * cosine) +
                (tagWeight * tagOverlap);

            return Math.Clamp(score / totalWeight, 0d, 1d);
        }

        private async Task<Dictionary<Guid, double>> CalculatePopularityScores(
            IReadOnlyList<Guid> courseIds,
            CancellationToken ct = default)
        {
            if (!courseIds.Any())
                return new Dictionary<Guid, double>();

            var rawPopularity = await _unitOfWork.Courses.GetCoursePopularityAsync(courseIds, ct);
            var scores = courseIds.ToDictionary(id => id, _ => 0d);

            if (!rawPopularity.Any())
                return scores;

            var maxEnrollments = rawPopularity.Max(x => x.EnrollmentCount);
            if (maxEnrollments <= 0)
                return scores;

            var denominator = Math.Log(1d + maxEnrollments);
            if (denominator <= 0)
                return scores;

            foreach (var item in rawPopularity)
            {
                var normalized = Math.Log(1d + Math.Max(0, item.EnrollmentCount)) / denominator;
                scores[item.CourseId] = Math.Clamp(normalized, 0d, 1d);
            }

            return scores;
        }

        private async Task<Dictionary<Guid, double>> CalculateQualityScores(
            IReadOnlyList<Guid> courseIds,
            CancellationToken ct = default)
        {
            if (!courseIds.Any())
                return new Dictionary<Guid, double>();

            var rawQuality = await _unitOfWork.Courses.GetCourseQualityAsync(courseIds, ct);
            var scores = courseIds.ToDictionary(id => id, _ => 0d);

            if (!rawQuality.Any())
                return scores;

            var configuredRatingWeight = Math.Max(0d, _recommendConfigurations.QualityRatingWeight);
            var configuredCompletionWeight = Math.Max(0d, _recommendConfigurations.QualityCompletionWeight);
            var weightSum = configuredRatingWeight + configuredCompletionWeight;

            var ratingWeight = weightSum > 0 ? configuredRatingWeight / weightSum : 0.7;
            var completionWeight = weightSum > 0 ? configuredCompletionWeight / weightSum : 0.3;

            foreach (var item in rawQuality)
            {
                var normalizedRating = Math.Clamp(item.AverageRating / 5d, 0d, 1d);
                var normalizedCompletion = Math.Clamp(item.CompletionRate, 0d, 1d);

                var qualityScore =
                    (ratingWeight * normalizedRating) +
                    (completionWeight * normalizedCompletion);

                scores[item.CourseId] = Math.Clamp(qualityScore, 0d, 1d);
            }

            return scores;
        }

        private async Task<Dictionary<Guid, double>> CalculateRecencyScores(
            IReadOnlyList<Guid> courseIds,
            CancellationToken ct = default)
        {
            if (!courseIds.Any())
                return new Dictionary<Guid, double>();

            var rawRecency = await _unitOfWork.Courses.GetCourseRecencyAsync(courseIds, ct);
            var scores = courseIds.ToDictionary(id => id, _ => 0d);

            if (!rawRecency.Any())
                return scores;

            // Keep recency in [0,1] using exponential decay by age.
            const double decayDays = 60d;
            var nowUtc = DateTime.UtcNow;

            foreach (var item in rawRecency)
            {
                var freshnessAt = new[]
                {
                    item.CreatedAt,
                    item.UpdatedAt,
                    item.LastTagUpdatedAt ?? item.UpdatedAt
                }.Max();

                var freshnessAtUtc = freshnessAt.Kind switch
                {
                    DateTimeKind.Utc => freshnessAt,
                    DateTimeKind.Local => freshnessAt.ToUniversalTime(),
                    _ => DateTime.SpecifyKind(freshnessAt, DateTimeKind.Utc)
                };

                var ageDays = Math.Max(0d, (nowUtc - freshnessAtUtc).TotalDays);
                scores[item.CourseId] = Math.Clamp(Math.Exp(-(ageDays / decayDays)), 0d, 1d);
            }

            return scores;
        }
    }
}
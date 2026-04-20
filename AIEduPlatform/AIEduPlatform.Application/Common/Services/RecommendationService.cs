using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEduPlatform.Application.Common.Services
{
    public class RecommendationService : IRecommendationService
    {
        private const int FinalSelectionSeedSalt = 2003;

        private readonly ILogger<RecommendationService> _logger;
        private readonly RecommendationWeightsDto _recommendConfigurations;
        private readonly CandidateGenerationDto _candidateConfigurations;
        private readonly RecommendationCandidateGenerator _candidateGenerator;
        private readonly RecommendationScoringEngine _scoringEngine;

        public RecommendationService(
            IOptions<RecommendationWeightsDto> recommendConfigurations,
            IOptions<CandidateGenerationDto> candidateConfigurations,
            IUnitOfWork unitOfWork,
            ILogger<RecommendationService> logger,
            ILoggerFactory loggerFactory)
        {
            _recommendConfigurations = recommendConfigurations.Value;
            _candidateConfigurations = candidateConfigurations.Value;
            _logger = logger;

            var candidateLogger = loggerFactory.CreateLogger<RecommendationCandidateGenerator>();
            var scoringLogger = loggerFactory.CreateLogger<RecommendationScoringEngine>();

            _candidateGenerator = new RecommendationCandidateGenerator(unitOfWork, _candidateConfigurations, candidateLogger);
            _scoringEngine = new RecommendationScoringEngine(unitOfWork, _recommendConfigurations, scoringLogger);
        }

        public async Task<List<Guid>> GetRecommendedCoursesAsync(
            Guid userId,
            int top = 10,
            CancellationToken ct = default)
        {
            var effectiveTop = top > 0
                ? top
                : Math.Max(1, _recommendConfigurations.TopK);

            var configuredCandidateLimit = _candidateConfigurations.CandidateLimit <= 0
                ? 100
                : _candidateConfigurations.CandidateLimit;

            var requestedCandidateTake = Math.Max(effectiveTop, Math.Min(configuredCandidateLimit, 100));

            var candidates = await _candidateGenerator.FindCandidateCoursesAsync(
                userId,
                requestedCandidateTake,
                ct);

            if (!candidates.Any())
            {
                _logger.LogInformation(
                    "Recommendation pipeline returned no candidates for user {UserId}.",
                    userId);

                return new List<Guid>();
            }

            var scored = await _scoringEngine.ScoreCoursesAsync(userId, candidates, ct);

            var ordered = scored
                .OrderByDescending(x => x.FinalScore)
                .ToList();

            if (!ordered.Any())
            {
                _logger.LogWarning(
                    "Scoring produced no rows for user {UserId}. Returning empty recommendation set (fallback disabled).",
                    userId);

                return new List<Guid>();
            }

            LogTopScoreBreakdown(userId, ordered, Math.Min(10, ordered.Count));

            var randomnessFactor = Math.Clamp(_recommendConfigurations.RandomnessFactor, 0d, 1d);
            var takeRandom = (int)Math.Round(effectiveTop * randomnessFactor, MidpointRounding.AwayFromZero);
            takeRandom = Math.Clamp(takeRandom, 0, effectiveTop);
            var takeTop = effectiveTop - takeRandom;

            var rankedCourseIds = ordered
                .Select(x => x.CourseId)
                .ToList();

            var topSelected = takeTop > 0
                ? rankedCourseIds.Take(takeTop).ToList()
                : new List<Guid>();

            var selectedSet = topSelected.ToHashSet();

            var randomPool = rankedCourseIds
                .Where(id => !selectedSet.Contains(id))
                .ToList();

            var randomSelected = RecommendationRandomUtils.ShuffleDeterministically(
                    randomPool,
                    userId,
                    FinalSelectionSeedSalt)
                .Take(takeRandom)
                .ToList();

            var merged = topSelected
                .Concat(randomSelected)
                .Distinct()
                .Take(effectiveTop)
                .ToList();

            var final = merged
                .Take(effectiveTop)
                .ToList();

            _logger.LogInformation(
                "Final recommendations for user {UserId}. RequestedTop: {Top}, Deterministic: {TakeTop}, Random: {TakeRandom}, CandidateCount: {CandidateCount}, Courses: {CourseIds}",
                userId,
                effectiveTop,
                takeTop,
                takeRandom,
                candidates.Count,
                string.Join(",", final));

            return final;
        }

        public Task<List<CourseScoreDto>> ScoreCoursesAsync(
            Guid userId,
            IEnumerable<Guid> candidateCourseIds,
            CancellationToken ct = default)
        {
            return _scoringEngine.ScoreCoursesAsync(userId, candidateCourseIds, ct);
        }

        public Task<List<Guid>> FindCandidateCoursesAsync(Guid userId, int take, CancellationToken ct = default)
        {
            return _candidateGenerator.FindCandidateCoursesAsync(userId, take, ct);
        }

        private void LogTopScoreBreakdown(Guid userId, IReadOnlyCollection<CourseScoreDto> orderedScores, int take)
        {
            if (orderedScores.Count == 0 || take <= 0)
                return;

            var breakdown = string.Join(" | ",
                orderedScores
                    .Take(take)
                    .Select(x =>
                        $"{x.CourseId}:final={x.FinalScore:F4},sim={x.Similarity:F4},qual={x.Quality:F4},pop={x.Popularity:F4},rec={x.Recency:F4}"));

            _logger.LogInformation(
                "Recommendation score breakdown for user {UserId}: {Breakdown}",
                userId,
                breakdown);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.DTOs.Recommedation
{
    public class RecommendationWeightsDto
    {
        /// <summary>
        /// User interest similarity (tags / embeddings)
        /// </summary>
        public double SimilarityWeight { get; set; } = 0.4;

        /// <summary>
        /// Course quality (rating + completion rate)
        /// </summary>
        public double QualityWeight { get; set; } = 0.2;

        /// <summary>
        /// Relative weight of average rating inside quality score.
        /// </summary>
        public double QualityRatingWeight { get; set; } = 0.7;

        /// <summary>
        /// Relative weight of completion rate inside quality score.
        /// </summary>
        public double QualityCompletionWeight { get; set; } = 0.3;

        /// <summary>
        /// Bayesian prior count for rating smoothing.
        /// </summary>
        public double QualityBayesianPriorCount { get; set; } = 5;

        /// <summary>
        /// Bayesian global mean for ratings in 0-5 scale (0 uses dataset-driven mean).
        /// </summary>
        public double QualityBayesianGlobalMean { get; set; } = 0;

        /// <summary>
        /// Relative contribution of cosine similarity in final similarity signal.
        /// </summary>
        public double SimilarityCosineWeight { get; set; } = 0.5;

        /// <summary>
        /// Relative contribution of tag-overlap similarity in final similarity signal.
        /// </summary>
        public double SimilarityTagOverlapWeight { get; set; } = 0.2;

        /// <summary>
        /// Relative contribution of reranker score in final similarity signal.
        /// </summary>
        public double SimilarityRerankWeight { get; set; } = 0.3;

        /// <summary>
        /// Course popularity (views, enrollments)
        /// </summary>
        public double PopularityWeight { get; set; } = 0.15;

        /// <summary>
        /// Course recency (newness boost)
        /// </summary>
        public double RecencyWeight { get; set; } = 0.15;

        /// <summary>
        /// Percentage of randomness (0 → no randomness, 0.2 → 20%)
        /// </summary>
        public double RandomnessFactor { get; set; } = 0.2;

        /// <summary>
        /// Final number of recommended courses
        /// </summary>
        public int TopK { get; set; } = 10;

        /// <summary>
        /// Maximum number of recommendations from the same primary tag cluster.
        /// </summary>
        public int MaxCoursesPerTagCluster { get; set; } = 2;

        /// <summary>
        /// When enabled, candidates below <see cref="MinimumSimilarityScore"/> are removed
        /// before final deterministic/random selection.
        /// </summary>
        public bool EnforceRelevanceFilter { get; set; } = true;

        /// <summary>
        /// Minimum similarity score required for a course to remain recommendation-eligible.
        /// Value is clamped to [0,1].
        /// </summary>
        public double MinimumSimilarityScore { get; set; } = 0.05;
    }
    public class CandidateGenerationDto
    {
        /// <summary>
        /// Percentage of candidates from similar courses (tag/user-based similarity)
        /// </summary>
        public double SimilarRatio { get; set; } = 0.4;

        /// <summary>
        /// Percentage of candidates from popular courses
        /// </summary>
        public double PopularRatio { get; set; } = 0.3;

        /// <summary>
        /// Percentage of candidates from newly created courses
        /// </summary>
        public double NewRatio { get; set; } = 0.2;

        /// <summary>
        /// Maximum number of candidate courses to fetch before scoring
        /// </summary>
        public int CandidateLimit { get; set; } = 100;

        /// <summary>
        /// Number of top cosine candidates passed to reranker for semantic refinement.
        /// </summary>
        public int RerankTopK { get; set; } = 30;

        /// <summary>
        /// Hard per-request cap for reranked candidates to control latency.
        /// </summary>
        public int RerankMaxPerRequest { get; set; } = 10;

        /// <summary>
        /// Relative source weight for similar-tag candidates.
        /// </summary>
        public double SimilarSourceWeight { get; set; } = 1.0;

        /// <summary>
        /// Relative source weight for popularity candidates.
        /// </summary>
        public double PopularSourceWeight { get; set; } = 0.5;

        /// <summary>
        /// Relative source weight for newest-course candidates.
        /// </summary>
        public double NewSourceWeight { get; set; } = 0.3;

        /// <summary>
        /// Portion of candidates taken deterministically from top-ranked list.
        /// </summary>
        public double DeterministicCandidateRatio { get; set; } = 0.8;

        /// <summary>
        /// Ratio where exploratory candidate pool starts (e.g. 0.75 means lower quartile).
        /// </summary>
        public double ExploratoryPoolStartRatio { get; set; } = 0.75;

        /// <summary>
        /// Portion of rerank pool seeded from top cosine results.
        /// </summary>
        public double RerankCosineSeedRatio { get; set; } = 0.7;

        /// <summary>
        /// Multiplier for similarity candidate prefetch volume.
        /// </summary>
        public int SimilarCandidateMultiplier { get; set; } = 5;

        /// <summary>
        /// Hard cap for similarity candidate prefetch.
        /// </summary>
        public int SimilarCandidateMaxFetch { get; set; } = 200;

        /// <summary>
        /// Whether to min-max normalize similarity after rerank blending.
        /// </summary>
        public bool NormalizeSimilarityAfterRerank { get; set; } = true;
    }
}

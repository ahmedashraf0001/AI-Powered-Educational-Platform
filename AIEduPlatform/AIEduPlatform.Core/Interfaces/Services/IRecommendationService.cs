using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IRecommendationService
    {
        Task<List<CourseScoreDto>> ScoreCoursesAsync(
            Guid userId,
            IEnumerable<Guid> candidateCourseIds,
            CancellationToken ct = default);

        Task<List<Guid>> GetRecommendedCoursesAsync(
            Guid userId,
            int top = 10,
            CancellationToken ct = default);
    }
    public class CourseScoreDto
    {
        public Guid CourseId { get; set; }

        public double Similarity { get; set; }
        public double Quality { get; set; }
        public double Popularity { get; set; }
        public double Recency { get; set; }

        public double FinalScore { get; set; }
    }
}

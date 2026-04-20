using AIEduPlatform.Core.DTOs.Progress;

namespace AIEduPlatform.Core.DTOs.Courses
{
    public record RecommendationSectionsDto
    {
        public List<CourseListDto> TopPicksForYou { get; init; } = new();
        public List<ContinueLearningDto> ContinueLearning { get; init; } = new();
        public string? BecauseYouLearnedCourseTitle { get; init; }
        public List<CourseListDto> BecauseYouLearned { get; init; } = new();
        public List<CourseListDto> TopCourses { get; init; } = new();
        public List<CourseListDto> TrendingCourses { get; init; } = new();
    }
}
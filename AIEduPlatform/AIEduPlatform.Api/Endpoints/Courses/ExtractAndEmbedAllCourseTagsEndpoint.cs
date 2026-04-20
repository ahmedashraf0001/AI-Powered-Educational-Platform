using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class ExtractAndEmbedAllCourseTagsFailure
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class ExtractAndEmbedAllCourseTagsResponse
{
    public int TotalCourses { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
    public List<ExtractAndEmbedAllCourseTagsFailure> Failures { get; set; } = new();
}

public class ExtractAndEmbedAllCourseTagsEndpoint
    : EndpointWithoutRequest<ApiResponse<ExtractAndEmbedAllCourseTagsResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITagExtractionService _tagExtractionService;
    private readonly ILogger<ExtractAndEmbedAllCourseTagsEndpoint> _logger;

    public ExtractAndEmbedAllCourseTagsEndpoint(
        IUnitOfWork unitOfWork,
        ITagExtractionService tagExtractionService,
        ILogger<ExtractAndEmbedAllCourseTagsEndpoint> logger)
    {
        _unitOfWork = unitOfWork;
        _tagExtractionService = tagExtractionService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/courses/tags/extract-and-embed-all");
        Roles("Teacher", "Admin");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Extract and embed tags for all courses";
            s.Description = "Runs full tag extraction and tag embedding refresh for every current course.";
            s.Response<ApiResponse<ExtractAndEmbedAllCourseTagsResponse>>(200, "Batch processing completed");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher or Admin role required");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var courses = (await _unitOfWork.Courses.GetAllAsync(ct))
            .OrderBy(c => c.CreatedAt)
            .ThenBy(c => c.Title)
            .ToList();

        var result = new ExtractAndEmbedAllCourseTagsResponse
        {
            TotalCourses = courses.Count,
            ProcessedAtUtc = DateTime.UtcNow
        };

        if (!courses.Any())
        {
            await SendOkAsync(
                ApiResponse<ExtractAndEmbedAllCourseTagsResponse>.Ok(result, "No courses found."),
                ct);
            return;
        }

        foreach (var course in courses)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                await _tagExtractionService.ExtractCourseTagsAsync(course.Id, ct);

                // Keep course rebuild state in sync for manual bulk runs.
                course.NeedsTagRebuild = false;
                course.PendingContentChanges = 0;
                course.HasContentDeletions = false;
                course.LastTagUpdatedAt = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync(ct);
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.Failures.Add(new ExtractAndEmbedAllCourseTagsFailure
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    Error = ex.Message
                });

                _logger.LogError(
                    ex,
                    "Bulk tag extraction failed for CourseId={CourseId}, Title={Title}",
                    course.Id,
                    course.Title);
            }
        }

        result.ProcessedAtUtc = DateTime.UtcNow;

        var message = result.Failed == 0
            ? $"Successfully extracted and embedded tags for {result.Succeeded} course(s)."
            : $"Completed with partial failures. Succeeded: {result.Succeeded}, Failed: {result.Failed}.";

        await SendOkAsync(ApiResponse<ExtractAndEmbedAllCourseTagsResponse>.Ok(result, message), ct);
    }
}
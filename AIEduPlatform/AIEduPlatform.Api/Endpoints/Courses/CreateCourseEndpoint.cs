using AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid? CategoryId { get; set; }
    public IFormFile? Thumbnail { get; set; }
}

public class CreateCourseResponse
{
    public Guid CourseId { get; set; }
}

public class CreateCourseEndpoint : Endpoint<CreateCourseRequest, ApiResponse<CreateCourseResponse>>
{
    private readonly IMediator _mediator;

    public CreateCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses");
        Roles("Teacher");
        AllowFormData();
        AllowFileUploads();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Create a new course";
            s.Description = "Creates a new course with optional thumbnail image. The authenticated teacher becomes the course instructor. Requires Teacher role.";
            s.Response<ApiResponse<CreateCourseResponse>>(201, "Course created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher role required");
        });
    }

    public override async Task HandleAsync(CreateCourseRequest req, CancellationToken ct)
    {
        Stream? thumbnailStream = null;
        string? thumbnailFileName = null;
        string? thumbnailContentType = null;

        if (req.Thumbnail != null && req.Thumbnail.Length > 0)
        {
            var ms = new MemoryStream();
            await req.Thumbnail.CopyToAsync(ms, ct);
            ms.Position = 0;
            thumbnailStream = ms;
            thumbnailFileName = req.Thumbnail.FileName;
            thumbnailContentType = req.Thumbnail.ContentType;
        }

        var courseId = await _mediator.Send(new CreateCourseCommand
        {
            Title = req.Title,
            Description = req.Description,
            Price = req.Price,
            CategoryId = req.CategoryId,
            ThumbnailStream = thumbnailStream,
            ThumbnailFileName = thumbnailFileName,
            ThumbnailContentType = thumbnailContentType
        }, ct);
        
        await SendCreatedAtAsync<GetCourseByIdEndpoint>(
            new { courseId },
            ApiResponse<CreateCourseResponse>.Ok(new CreateCourseResponse { CourseId = courseId }, "Course created successfully."),
            cancellation: ct);
    }
}

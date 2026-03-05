using AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class UpdateCourseRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public Guid? CategoryId { get; set; }
    public IFormFile? Thumbnail { get; set; }
    public bool RemoveThumbnail { get; set; }
}

public class UpdateCourseEndpoint : Endpoint<UpdateCourseRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/courses/{CourseId}");
        Roles("Teacher");
        AllowFormData();
        AllowFileUploads();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Update a course";
            s.Description = "Updates a course including optional thumbnail image. Set RemoveThumbnail=true to remove the current thumbnail. Only the course instructor can update it.";
            s.Response(204, "Course updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(UpdateCourseRequest req, CancellationToken ct)
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

        await _mediator.Send(new UpdateCourseCommand
        {
            CourseId = req.CourseId,
            Title = req.Title,
            Description = req.Description,
            Price = req.Price,
            CategoryId = req.CategoryId,
            ThumbnailStream = thumbnailStream,
            ThumbnailFileName = thumbnailFileName,
            ThumbnailContentType = thumbnailContentType,
            RemoveThumbnail = req.RemoveThumbnail
        }, ct);

        await SendNoContentAsync(ct);
    }
}

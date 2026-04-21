using AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid? CategoryId { get; set; }
    public List<Guid>? CategoryIds { get; set; }
    public IFormFile? Thumbnail { get; set; }
}

public class CreateCourseResponse
{
    public Guid CourseId { get; set; }
}

public class CreateCourseEndpoint : Endpoint<CreateCourseRequest, ApiResponse<CreateCourseResponse>>
{
    private readonly IMediator _mediator;
    private static readonly Regex GuidRegex = new(
        @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}",
        RegexOptions.Compiled);

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

        var categoryIds = ResolveCategoryIds(req);

        var courseId = await _mediator.Send(new CreateCourseCommand
        {
            Title = req.Title,
            Description = req.Description,
            Price = req.Price,
            CategoryIds = categoryIds,
            ThumbnailStream = thumbnailStream,
            ThumbnailFileName = thumbnailFileName,
            ThumbnailContentType = thumbnailContentType
        }, ct);
        
        await SendCreatedAtAsync<GetCourseByIdEndpoint>(
            new { courseId },
            ApiResponse<CreateCourseResponse>.Ok(new CreateCourseResponse { CourseId = courseId }, "Course created successfully."),
            cancellation: ct);
    }

    private List<Guid>? ResolveCategoryIds(CreateCourseRequest req)
    {
        var ids = new HashSet<Guid>();

        if (req.CategoryId.HasValue)
            ids.Add(req.CategoryId.Value);

        if (req.CategoryIds != null)
        {
            foreach (var id in req.CategoryIds)
                ids.Add(id);
        }

        if (HttpContext.Request.HasFormContentType)
        {
            var form = HttpContext.Request.Form;
            AddFromValues(form["CategoryId"], ids);
            AddFromValues(form["categoryId"], ids);
            AddFromValues(form["CategoryIds"], ids);
            AddFromValues(form["categoryIds"], ids);

            foreach (var key in form.Keys)
            {
                if (key.StartsWith("CategoryIds[", StringComparison.OrdinalIgnoreCase) ||
                    key.StartsWith("categoryIds[", StringComparison.OrdinalIgnoreCase))
                {
                    AddFromValues(form[key], ids);
                }
            }
        }

        return ids.Count > 0 ? ids.ToList() : null;
    }

    private static void AddFromValues(StringValues values, HashSet<Guid> ids)
    {
        foreach (var raw in values)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            foreach (Match match in GuidRegex.Matches(raw))
            {
                if (Guid.TryParse(match.Value, out var parsed))
                    ids.Add(parsed);
            }
        }
    }
}

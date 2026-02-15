using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCourseById;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCourseByIdRequest
{
    public Guid CourseId { get; set; }
}

public class GetCourseByIdEndpoint : Endpoint<GetCourseByIdRequest, ApiResponse<CourseDetailDto>>
{
    private readonly IMediator _mediator;

    public GetCourseByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}");
        AllowAnonymous();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get course details";
            s.Description = "Returns course metadata and ordered lecture titles. Use GetCourseReviews endpoint for reviews and GetLectureById for materials.";
            s.Response<ApiResponse<CourseDetailDto>>(200, "Course details");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery
        {
            CourseId = req.CourseId
        }, ct);
        await SendOkAsync(ApiResponse<CourseDetailDto>.Ok(result), ct);
    }
}

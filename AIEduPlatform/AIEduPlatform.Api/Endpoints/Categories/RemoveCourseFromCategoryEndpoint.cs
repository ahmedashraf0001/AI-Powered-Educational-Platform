using AIEduPlatform.Application.Features.Courses.Commands.Categories.RemoveCourseFromCategory;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class RemoveCourseFromCategoryRequest
{
    public Guid CourseId { get; set; }
    public Guid CategoryId { get; set; }
}

public class RemoveCourseFromCategoryEndpoint : Endpoint<RemoveCourseFromCategoryRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public RemoveCourseFromCategoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/{CourseId}/categories/{CategoryId}");
        Roles("Teacher");
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Remove course from category";
            s.Description = "Removes a course-category association. Teachers only.";
            s.Response<ApiResponse<object>>(200, "Course removed from category");
            s.Response(404, "Association not found");
        });
    }

    public override async Task HandleAsync(RemoveCourseFromCategoryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new RemoveCourseFromCategoryCommand
        {
            CourseId = req.CourseId,
            CategoryId = req.CategoryId
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Course removed from category successfully."), ct);
    }
}

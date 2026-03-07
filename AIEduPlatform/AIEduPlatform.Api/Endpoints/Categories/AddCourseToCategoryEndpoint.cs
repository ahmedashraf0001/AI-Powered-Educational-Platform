using AIEduPlatform.Application.Features.Courses.Commands.Categories.AddCourseToCategory;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class AddCourseToCategoryRequest
{
    public Guid CourseId { get; set; }
    public Guid CategoryId { get; set; }
}

public class AddCourseToCategoryEndpoint : Endpoint<AddCourseToCategoryRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public AddCourseToCategoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/categories");
        Roles("Teacher");
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Add course to category";
            s.Description = "Associates a course with a category. Teachers only.";
            s.ExampleRequest = new AddCourseToCategoryRequest
            {
                CourseId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                CategoryId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901")
            };
            s.Response<ApiResponse<object>>(200, "Course added to category");
            s.Response(404, "Course or category not found");
            s.Response(409, "Association already exists");
        });
    }

    public override async Task HandleAsync(AddCourseToCategoryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new AddCourseToCategoryCommand
        {
            CourseId = req.CourseId,
            CategoryId = req.CategoryId
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Course added to category successfully."), ct);
    }
}

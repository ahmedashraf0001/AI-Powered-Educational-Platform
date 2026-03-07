using AIEduPlatform.Application.Features.Courses.Commands.Categories.CreateCategory;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CreateCategoryEndpoint : Endpoint<CreateCategoryRequest, ApiResponse<Guid>>
{
    private readonly IMediator _mediator;

    public CreateCategoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/categories");
        Roles("Teacher");
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Create a category";
            s.Description = "Creates a new course category. Teachers only.";
            s.ExampleRequest = new CreateCategoryRequest
            {
                Name = "Machine Learning",
                Description = "Courses covering ML algorithms, deep learning, and neural networks"
            };
            s.Response<ApiResponse<Guid>>(200, "Category created");
            s.Response(400, "Validation error");
            s.Response(409, "Category with this name already exists");
        });
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        var categoryId = await _mediator.Send(new CreateCategoryCommand
        {
            Name = req.Name,
            Description = req.Description
        }, ct);

        await SendOkAsync(ApiResponse<Guid>.Ok(categoryId, "Category created successfully."), ct);
    }
}

using AIEduPlatform.Application.Features.Courses.Commands.Categories.UpdateCategory;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class UpdateCategoryRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateCategoryEndpoint : Endpoint<UpdateCategoryRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public UpdateCategoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/categories/{CategoryId}");
        Roles("Teacher");
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Update a category";
            s.Description = "Updates an existing category. Teachers only.";
            s.ExampleRequest = new UpdateCategoryRequest
            {
                CategoryId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Name = "Deep Learning",
                Description = "Advanced courses on neural networks and deep learning frameworks"
            };
            s.Response<ApiResponse<object>>(200, "Category updated");
            s.Response(404, "Category not found");
            s.Response(409, "Category name conflict");
        });
    }

    public override async Task HandleAsync(UpdateCategoryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateCategoryCommand
        {
            CategoryId = req.CategoryId,
            Name = req.Name,
            Description = req.Description
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Category updated successfully."), ct);
    }
}

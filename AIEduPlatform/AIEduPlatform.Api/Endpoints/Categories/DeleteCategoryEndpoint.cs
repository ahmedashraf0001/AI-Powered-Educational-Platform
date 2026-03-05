using AIEduPlatform.Application.Features.Courses.Commands.Categories.DeleteCategory;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class DeleteCategoryRequest
{
    public Guid CategoryId { get; set; }
}

public class DeleteCategoryEndpoint : Endpoint<DeleteCategoryRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public DeleteCategoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/categories/{CategoryId}");
        Roles("Teacher");
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a category";
            s.Description = "Deletes a category and all its course associations. Teachers only.";
            s.Response<ApiResponse<object>>(200, "Category deleted");
            s.Response(404, "Category not found");
        });
    }

    public override async Task HandleAsync(DeleteCategoryRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCategoryCommand
        {
            CategoryId = req.CategoryId
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Category deleted successfully."), ct);
    }
}

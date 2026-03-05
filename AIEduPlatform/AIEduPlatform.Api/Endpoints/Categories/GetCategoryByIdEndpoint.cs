using AIEduPlatform.Application.Features.Courses.Queries.Categories.GetCategoryById;
using AIEduPlatform.Core.DTOs.Categories;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class GetCategoryByIdRequest
{
    public Guid CategoryId { get; set; }
}

public class GetCategoryByIdEndpoint : Endpoint<GetCategoryByIdRequest, ApiResponse<CategoryDto>>
{
    private readonly IMediator _mediator;

    public GetCategoryByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/categories/{CategoryId}");
        AllowAnonymous();
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Get category by ID";
            s.Description = "Returns a single category by its ID.";
            s.Response<ApiResponse<CategoryDto>>(200, "Category details");
            s.Response(404, "Category not found");
        });
    }

    public override async Task HandleAsync(GetCategoryByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery
        {
            CategoryId = req.CategoryId
        }, ct);

        await SendOkAsync(ApiResponse<CategoryDto>.Ok(result), ct);
    }
}

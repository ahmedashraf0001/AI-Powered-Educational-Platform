using AIEduPlatform.Application.Features.Courses.Queries.Categories.GetAllCategories;
using AIEduPlatform.Core.DTOs.Categories;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Categories;

public class GetAllCategoriesRequest
{
    [QueryParam]
    public string? SearchTerm { get; set; }
}

public class GetAllCategoriesEndpoint : Endpoint<GetAllCategoriesRequest, ApiResponse<List<CategoryDto>>>
{
    private readonly IMediator _mediator;

    public GetAllCategoriesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/categories");
        AllowAnonymous();
        Group<CategoriesGroup>();
        Summary(s =>
        {
            s.Summary = "Get all categories";
            s.Description = "Returns all categories with optional search. No authentication required.";
            s.Response<ApiResponse<List<CategoryDto>>>(200, "List of categories");
        });
    }

    public override async Task HandleAsync(GetAllCategoriesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery
        {
            SearchTerm = req.SearchTerm
        }, ct);

        await SendOkAsync(ApiResponse<List<CategoryDto>>.Ok(result), ct);
    }
}

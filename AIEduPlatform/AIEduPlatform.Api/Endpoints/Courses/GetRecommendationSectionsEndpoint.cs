using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendationSections;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetRecommendationSectionsRequest
{
    [QueryParam]
    public int? Top { get; set; }
}

public class GetRecommendationSectionsEndpoint : Endpoint<GetRecommendationSectionsRequest, ApiResponse<RecommendationSectionsDto>>
{
    private readonly IMediator _mediator;

    public GetRecommendationSectionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/recommended/sections");
        Roles("Student", "Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get recommendation sections";
            s.Description = "Returns sectioned recommendations for the authenticated user dashboard.";
            s.Response<ApiResponse<RecommendationSectionsDto>>(200, "Sectioned recommendations");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetRecommendationSectionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRecommendationSectionsQuery
        {
            Top = req.Top ?? 10
        }, ct);

        await SendOkAsync(ApiResponse<RecommendationSectionsDto>.Ok(result), ct);
    }
}
using AIEduPlatform.Application.Features.Courses.Queries.Materials.GetMaterialProjection;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Materials;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class GetMaterialProjectionRequest
{
    public Guid MaterialId { get; set; }
}

public class GetMaterialProjectionEndpoint : Endpoint<GetMaterialProjectionRequest, ApiResponse<MaterialProjectionDto>>
{
    private readonly IMediator _mediator;

    public GetMaterialProjectionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/materials/{MaterialId}/projection");
        Roles("Student", "Teacher");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Get material projection";
            s.Description = "Returns material metadata with progress and resume position. Read-only, no side effects.";
            s.Response<ApiResponse<MaterialProjectionDto>>(200, "Material projection");
            s.Response(400, "Not enrolled");
            s.Response(401, "Not authenticated");
            s.Response(404, "Material not found");
        });
    }

    public override async Task HandleAsync(GetMaterialProjectionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetMaterialProjectionQuery
        {
            MaterialId = req.MaterialId
        }, ct);

        await SendOkAsync(ApiResponse<MaterialProjectionDto>.Ok(result), ct);
    }
}

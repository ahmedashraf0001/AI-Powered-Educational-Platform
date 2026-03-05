using AIEduPlatform.Application.Features.StudySessions.Queries.Sections.GetSectionsByMaterial;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Materials;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Sections;

public class GetSectionsByMaterialRequest
{
    public Guid MaterialId { get; set; }
}

public class GetSectionsByMaterialEndpoint : Endpoint<GetSectionsByMaterialRequest, ApiResponse<List<SemanticSectionDto>>>
{
    private readonly IMediator _mediator;

    public GetSectionsByMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/materials/{MaterialId}/sections");
        Group<SectionsGroup>();
        Roles("Student", "Teacher");
        Summary(s =>
        {
            s.Summary = "Get semantic sections for a material";
            s.Description = "Returns all semantic sections extracted from a material, ordered by position.";
            s.Response<ApiResponse<List<SemanticSectionDto>>>(200, "Semantic sections");
            s.Response(401, "Not authenticated");
            s.Response(404, "Material not found");
        });
    }

    public override async Task HandleAsync(GetSectionsByMaterialRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSectionsByMaterialQuery
        {
            MaterialId = req.MaterialId
        }, ct);

        await SendOkAsync(ApiResponse<List<SemanticSectionDto>>.Ok(result), ct);
    }
}

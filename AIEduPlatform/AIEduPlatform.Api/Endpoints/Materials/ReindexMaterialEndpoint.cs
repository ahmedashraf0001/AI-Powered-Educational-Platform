using AIEduPlatform.Application.Features.Courses.Commands.Materials.ReindexMaterial;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class ReindexMaterialEndpoint : EndpointWithoutRequest<ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public ReindexMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/materials/{Id}/reindex");
        Group<MaterialsGroup>();
        Roles("Teacher", "Admin");
        Summary(s =>
        {
            s.Summary = "Reindex a material";
            s.Description = "Puts a specific material into the indexing queue to be processed again. Required if it's in a failed state.";
            s.Response<ApiResponse<object>>(200, "Material queued for re-indexing");
            s.Response(404, "Material not found");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var materialId = Route<Guid>("Id");
        await _mediator.Send(new ReindexMaterialCommand { MaterialId = materialId }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Material queued for re-indexing."), ct);
    }
}

using AIEduPlatform.Application.Features.Courses.Commands.Materials.DeleteMaterial;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class DeleteMaterialRequest
{
    public Guid MaterialId { get; set; }
}

public class DeleteMaterialEndpoint : Endpoint<DeleteMaterialRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/materials/{materialId}");
        Group<MaterialsGroup>();
    }

    public override async Task HandleAsync(DeleteMaterialRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMaterialCommand { MaterialId = req.MaterialId }, ct);
        await SendNoContentAsync(ct);
    }
}

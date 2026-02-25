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
        Delete("/api/courses/materials/{MaterialId}");
        Roles("Teacher");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a material";
            s.Description = "Permanently deletes a course material. Only the course instructor can delete it.";
            s.Response(204, "Material deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(DeleteMaterialRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteMaterialCommand { MaterialId = req.MaterialId }, ct);
        await SendNoContentAsync(ct);
    }
}

using AIEduPlatform.Application.Features.Courses.Commands.Materials.UpdateMaterialProgress;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class UpdateMaterialProgressRequest
{
    public Guid MaterialId { get; set; }
    public int Position { get; set; }
}

public class UpdateMaterialProgressEndpoint : Endpoint<UpdateMaterialProgressRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public UpdateMaterialProgressEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/materials/{MaterialId}/progress");
        Roles("Student");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Update material progress";
            s.Description = "Updates the student's progress position for a material. Uses conflict-safe update rule: only overwrites if new position is strictly greater.";
            s.Response<ApiResponse<object>>(200, "Progress updated");
            s.Response(400, "Not enrolled or invalid position");
            s.Response(401, "Not authenticated");
            s.Response(404, "Material not found");
        });
    }

    public override async Task HandleAsync(UpdateMaterialProgressRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateMaterialProgressCommand
        {
            MaterialId = req.MaterialId,
            Position = req.Position
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Progress updated successfully."), ct);
    }
}

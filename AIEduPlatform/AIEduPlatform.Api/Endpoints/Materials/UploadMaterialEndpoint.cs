using AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class UploadMaterialRequest
{
    public Guid LectureId { get; set; }
    public string Title { get; set; } = string.Empty;
    public MaterialType Type { get; set; }
    public IFormFile? File { get; set; }
    public string? FileUrl { get; set; }
}

public class UploadMaterialResponse
{
    public Guid MaterialId { get; set; }
}

public class UploadMaterialEndpoint : Endpoint<UploadMaterialRequest, UploadMaterialResponse>
{
    private readonly IMediator _mediator;

    public UploadMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/lectures/{LectureId}/materials");
        Roles("Teacher");
        AllowFormData();
        AllowFileUploads();
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Upload lecture material";
            s.Description = "Uploads a file or links a URL as course material. Only the course instructor can upload materials.";
            s.Response<UploadMaterialResponse>(201, "Material uploaded");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(UploadMaterialRequest req, CancellationToken ct)
    {
        var materialId = await _mediator.Send(new UploadMaterialCommand
        {
            LectureId = req.LectureId,
            Title = req.Title,
            Type = req.Type,
            FileUrl = req.FileUrl,
            FileStream = req.File?.OpenReadStream(),
            FileName = req.File?.FileName,
            ContentType = req.File?.ContentType
        }, ct);
        
        await SendCreatedAtAsync<GetLectureMaterialsEndpoint>(
            new { lectureId = req.LectureId },
            new UploadMaterialResponse { MaterialId = materialId },
            cancellation: ct);
    }
}

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
        Post("/api/courses/lectures/{lectureId}/materials");
        AllowFormData();
        AllowFileUploads();
        Group<MaterialsGroup>();
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

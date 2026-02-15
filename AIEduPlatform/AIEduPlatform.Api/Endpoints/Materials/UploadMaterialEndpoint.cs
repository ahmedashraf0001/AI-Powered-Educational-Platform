using AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class UploadMaterialRequest
{
    public Guid LectureId { get; set; }
    public List<IFormFile> Files { get; set; } = [];
    [QueryParam]
    public string Titles { get; set; } = string.Empty;
}

public class UploadMaterialResponse
{
    public List<Guid> MaterialIds { get; set; } = [];
}

public class UploadMaterialEndpoint : Endpoint<UploadMaterialRequest, ApiResponse<UploadMaterialResponse>>
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
            s.Summary = "Upload lecture materials (bulk)";
            s.Description = "Uploads one or more files as course materials. Provide comma-separated Titles matching the file order. Material type is inferred from file extension. Only the course instructor can upload.";
            s.Response<ApiResponse<UploadMaterialResponse>>(201, "Materials uploaded");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(UploadMaterialRequest req, CancellationToken ct)
    {
        if (req.Files == null || req.Files.Count == 0)
        {
            ThrowError("At least one file must be provided.");
            return;
        }

        var titles = req.Titles?.Split(',', StringSplitOptions.TrimEntries) ?? [];

        var files = req.Files.Select((f, i) =>
        {
            var title = i < titles.Length ? titles[i] : Path.GetFileNameWithoutExtension(f.FileName);

            return new UploadMaterialFile
            {
                Title = title,
                FileStream = f.OpenReadStream(),
                FileName = f.FileName,
                ContentType = f.ContentType
            };
        }).ToList();

        var materialIds = await _mediator.Send(new UploadMaterialCommand
        {
            LectureId = req.LectureId,
            Files = files
        }, ct);

        await SendCreatedAtAsync<GetLectureMaterialsEndpoint>(
            new { lectureId = req.LectureId },
            ApiResponse<UploadMaterialResponse>.Ok(new UploadMaterialResponse { MaterialIds = materialIds }, "Materials uploaded successfully."),
            cancellation: ct);
    }
}

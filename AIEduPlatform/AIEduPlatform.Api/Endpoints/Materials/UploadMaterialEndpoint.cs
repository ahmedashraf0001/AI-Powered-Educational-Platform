using AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.ML.Configurations;
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
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

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
            s.Description = $"Uploads one or more files as course materials. Provide comma-separated Titles matching the file order. Material type is inferred from file extension. Supported formats: {FileExtensionConfiguration.GetSupportedExtensionsString()}. Max file size: 100 MB. Only the course instructor can upload.";
            s.Response<ApiResponse<UploadMaterialResponse>>(201, "Materials uploaded");
            s.Response(400, "Invalid request or unsupported file type");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Lecture not found");
        });
    }

    public override async Task HandleAsync(UploadMaterialRequest req, CancellationToken ct)
    {
        if (req.Files == null || req.Files.Count == 0)
        {
            ThrowError("At least one file must be provided.");
            return;
        }

        // Validate file types and sizes early
        foreach (var file in req.Files)
        {
            if (!FileExtensionConfiguration.IsSupported(file.FileName))
            {
                ThrowError($"File '{file.FileName}' has an unsupported format. Allowed types: {FileExtensionConfiguration.GetSupportedExtensionsString()}");
                return;
            }

            if (file.Length > MaxFileSize)
            {
                ThrowError($"File '{file.FileName}' exceeds the maximum allowed size of 100 MB.");
                return;
            }

            if (file.Length == 0)
            {
                ThrowError($"File '{file.FileName}' is empty.");
                return;
            }
        }

        var titles = req.Titles?.Split(',', StringSplitOptions.TrimEntries) ?? [];

        // Create memory streams for all files to ensure they're seekable and reusable
        var files = new List<UploadMaterialFile>();

        try
        {
            for (int i = 0; i < req.Files.Count; i++)
            {
                var formFile = req.Files[i];
                var memoryStream = new MemoryStream();

                await formFile.CopyToAsync(memoryStream, ct);
                memoryStream.Position = 0; // Reset position for reading

                var title = i < titles.Length && !string.IsNullOrWhiteSpace(titles[i])
                    ? titles[i]
                    : Path.GetFileNameWithoutExtension(formFile.FileName);

                files.Add(new UploadMaterialFile
                {
                    Title = title,
                    FileStream = memoryStream,
                    FileName = formFile.FileName,
                    ContentType = formFile.ContentType
                });
            }

            var materialIds = await _mediator.Send(new UploadMaterialCommand
            {
                LectureId = req.LectureId,
                Files = files
            }, ct);

            await SendCreatedAtAsync<GetLectureMaterialsEndpoint>(
                new { lectureId = req.LectureId },
                ApiResponse<UploadMaterialResponse>.Ok(
                    new UploadMaterialResponse { MaterialIds = materialIds },
                    "Materials uploaded successfully."),
                cancellation: ct);
        }
        finally
        {
            // Clean up memory streams
            foreach (var file in files)
            {
                file.FileStream?.Dispose();
            }
        }
    }
}
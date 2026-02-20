using AIEduPlatform.Application.Features.Courses.Queries.Materials.StreamMaterial;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class DownloadMaterialRequest
{
    public Guid MaterialId { get; set; }
}

/// <summary>
/// Forces a file download for any material type (including videos, PDFs).
/// Unlike the stream endpoint which serves inline, this always sets Content-Disposition: attachment.
/// </summary>
public class DownloadMaterialEndpoint : Endpoint<DownloadMaterialRequest>
{
    private readonly IMediator _mediator;

    public DownloadMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/materials/{MaterialId}/download");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Download a material file";
            s.Description = "Downloads a material file as an attachment. User must be enrolled in the course or be the instructor.";
            s.Response(200, "File download");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
            s.Response(404, "Material or file not found");
        });
    }

    public override async Task HandleAsync(DownloadMaterialRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new StreamMaterialQuery
        {
            MaterialId = req.MaterialId
        }, ct);

        var fileInfo = new FileInfo(result.FilePath);

        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = result.ContentType;
        HttpContext.Response.ContentLength = fileInfo.Length;
        HttpContext.Response.Headers.Append("Content-Disposition",
            $"attachment; filename=\"{result.FileName}\"");

        await using var fileStream = new FileStream(
            result.FilePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await fileStream.CopyToAsync(HttpContext.Response.Body, ct);
    }
}

using AIEduPlatform.Application.Features.Courses.Queries.Materials.StreamMaterial;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;
using Microsoft.Net.Http.Headers;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class StreamMaterialRequest
{
    public Guid MaterialId { get; set; }
}

/// <summary>
/// Streams a material file with proper HTTP Range support for video/audio seeking.
/// Videos: browser &lt;video&gt; tag uses range requests for buffered playback.
/// Audio: browser &lt;audio&gt; tag uses range requests similarly.
/// PDFs: served whole, frontend renders with &lt;iframe&gt; or pdf.js.
/// Images: served whole, frontend uses &lt;img&gt; tag.
/// Documents: served as attachment download.
/// </summary>
public class StreamMaterialEndpoint : Endpoint<StreamMaterialRequest>
{
    private readonly IMediator _mediator;

    public StreamMaterialEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/materials/{MaterialId}/stream");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Stream or download a material file";
            s.Description = "Streams a material file with HTTP Range support for video/audio seeking. " +
                            "PDFs and images are served inline. Documents are served as downloads. " +
                            "User must be enrolled in the course or be the instructor.";
            s.Response(200, "Full file content");
            s.Response(206, "Partial content (range request)");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
            s.Response(404, "Material or file not found");
        });
    }

    public override async Task HandleAsync(StreamMaterialRequest req, CancellationToken ct)
    {
        // Resolve material, authorize user, get physical path
        var result = await _mediator.Send(new StreamMaterialQuery
        {
            MaterialId = req.MaterialId
        }, ct);

        var fileInfo = new FileInfo(result.FilePath);
        var fileLength = fileInfo.Length;
        var contentType = result.ContentType;

        // Set Content-Disposition based on material type
        var disposition = result.Type switch
        {
            MaterialType.Video => "inline",
            MaterialType.Audio => "inline",
            MaterialType.Image => "inline",
            _ when contentType == "application/pdf" => "inline",
            _ => "attachment"
        };

        HttpContext.Response.Headers.Append("Content-Disposition",
            $"{disposition}; filename=\"{result.FileName}\"");
        HttpContext.Response.Headers.Append("Accept-Ranges", "bytes");

        // Cache static files for 1 hour
        HttpContext.Response.Headers.Append("Cache-Control", "private, max-age=3600");

        // Check for Range header (used by <video> and <audio> tags for seeking)
        var rangeHeader = HttpContext.Request.Headers.Range.ToString();

        if (!string.IsNullOrEmpty(rangeHeader) && RangeHeaderValue.TryParse(rangeHeader, out var range))
        {
            var rangeItem = range.Ranges.FirstOrDefault();
            if (rangeItem != null)
            {
                var start = rangeItem.From ?? 0;
                var end = rangeItem.To ?? fileLength - 1;

                // Clamp end to file length
                if (end >= fileLength) end = fileLength - 1;

                var chunkSize = end - start + 1;

                HttpContext.Response.StatusCode = 206;
                HttpContext.Response.ContentType = contentType;
                HttpContext.Response.ContentLength = chunkSize;
                HttpContext.Response.Headers.Append("Content-Range",
                    $"bytes {start}-{end}/{fileLength}");

                await using var fileStream = new FileStream(
                    result.FilePath, FileMode.Open, FileAccess.Read,
                    FileShare.Read, bufferSize: 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

                fileStream.Seek(start, SeekOrigin.Begin);

                var buffer = new byte[64 * 1024]; // 64KB buffer
                var remaining = chunkSize;

                while (remaining > 0)
                {
                    var bytesToRead = (int)Math.Min(buffer.Length, remaining);
                    var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, bytesToRead), ct);
                    if (bytesRead == 0) break;

                    await HttpContext.Response.Body.WriteAsync(buffer.AsMemory(0, bytesRead), ct);
                    remaining -= bytesRead;
                }

                return;
            }
        }

        // No Range header — serve the full file
        HttpContext.Response.StatusCode = 200;
        HttpContext.Response.ContentType = contentType;
        HttpContext.Response.ContentLength = fileLength;

        await using var fullStream = new FileStream(
            result.FilePath, FileMode.Open, FileAccess.Read,
            FileShare.Read, bufferSize: 64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);

        await fullStream.CopyToAsync(HttpContext.Response.Body, ct);
    }
}

using AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureMaterials;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Materials;

public class GetLectureMaterialsRequest
{
    public Guid LectureId { get; set; }
}

public class GetLectureMaterialsEndpoint : Endpoint<GetLectureMaterialsRequest, List<MaterialDto>>
{
    private readonly IMediator _mediator;

    public GetLectureMaterialsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/lectures/{LectureId}/materials");
        Group<MaterialsGroup>();
        Summary(s =>
        {
            s.Summary = "Get lecture materials";
            s.Description = "Returns all materials for a lecture. User must be enrolled in the course or be the instructor.";
            s.Response<List<MaterialDto>>(200, "Lecture materials");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
        });
    }

    public override async Task HandleAsync(GetLectureMaterialsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLectureMaterialsQuery { LectureId = req.LectureId }, ct);
        await SendOkAsync(result, ct);
    }
}

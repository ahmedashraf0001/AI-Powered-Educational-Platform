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
        Get("/api/courses/lectures/{lectureId}/materials");
        Group<MaterialsGroup>();
    }

    public override async Task HandleAsync(GetLectureMaterialsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLectureMaterialsQuery { LectureId = req.LectureId }, ct);
        await SendOkAsync(result, ct);
    }
}

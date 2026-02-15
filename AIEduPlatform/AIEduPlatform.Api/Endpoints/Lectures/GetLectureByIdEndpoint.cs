using AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureById;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class GetLectureByIdRequest
{
    public Guid LectureId { get; set; }
}

public class GetLectureByIdEndpoint : Endpoint<GetLectureByIdRequest, ApiResponse<LectureDetailDto>>
{
    private readonly IMediator _mediator;

    public GetLectureByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/lectures/{LectureId}");
        Group<LecturesGroup>();
        Summary(s =>
        {
            s.Summary = "Get lecture details";
            s.Description = "Returns lecture details with materials categorized by type (Video, Document, Audio, Image). Must be enrolled or be the instructor.";
            s.Response<ApiResponse<LectureDetailDto>>(200, "Lecture details with categorized materials");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
            s.Response(404, "Lecture not found");
        });
    }

    public override async Task HandleAsync(GetLectureByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLectureByIdQuery
        {
            LectureId = req.LectureId
        }, ct);
        await SendOkAsync(ApiResponse<LectureDetailDto>.Ok(result), ct);
    }
}

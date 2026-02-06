using AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class UpdateLectureRequest
{
    public Guid LectureId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
}

public class UpdateLectureEndpoint : Endpoint<UpdateLectureRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateLectureEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/courses/lectures/{lectureId}");
        Group<LecturesGroup>();
    }

    public override async Task HandleAsync(UpdateLectureRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateLectureCommand
        {
            LectureId = req.LectureId,
            Title = req.Title,
            Description = req.Description,
            OrderIndex = req.OrderIndex
        }, ct);

        await SendNoContentAsync(ct);
    }
}

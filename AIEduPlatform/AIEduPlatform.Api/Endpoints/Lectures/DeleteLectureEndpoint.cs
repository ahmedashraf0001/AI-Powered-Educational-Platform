using AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class DeleteLectureRequest
{
    public Guid LectureId { get; set; }
}

public class DeleteLectureEndpoint : Endpoint<DeleteLectureRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteLectureEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/lectures/{lectureId}");
        Group<LecturesGroup>();
    }

    public override async Task HandleAsync(DeleteLectureRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteLectureCommand { LectureId = req.LectureId }, ct);
        await SendNoContentAsync(ct);
    }
}
